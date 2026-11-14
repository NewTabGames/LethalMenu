using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using LethalMenu.Manager;
using LethalMenu.Types;
using LethalMenu.Util;
using Newtonsoft.Json.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalMenu.Remote;

/// <summary>
/// In-game menu tools that are not plain features (the enemy spawner, the per-enemy buttons, the Moon / Weather /
/// Unlockable / Suit / Item manager windows), offered to the website as extra actions. They show up there like any
/// other feature, with their choices (which enemy, which item...) sent as dropdown lists.
/// Everything here runs on the main thread.
/// </summary>
internal static class RemoteExtras
{
	internal const string Prefix = "X_";

	private class Extra
	{
		public string Key;

		public string Name;

		public string Tab;

		public string Group;

		/// <summary>Only offer this on cards of this monster type, e.g. "SandSpiderAI" for web spawning.</summary>
		public string Only;

		/// <summary>Never offer this on cards of these monster types.</summary>
		public string[] Not;

		public Func<JArray> Args = () => new JArray();

		public Func<JArray, string> Run;
	}

	private static JObject Choice(string name, IEnumerable<string> options)
	{
		return new JObject { ["name"] = name, ["kind"] = "enum", ["options"] = new JArray(options.Where((string o) => !string.IsNullOrEmpty(o)).Distinct().OrderBy((string o) => o, StringComparer.OrdinalIgnoreCase)) };
	}

	/// <summary>A whole number that may be left empty on the website (the hint says what empty means).</summary>
	private static JObject Number(string name, string hint)
	{
		return new JObject { ["name"] = name, ["kind"] = "number", ["integer"] = true, ["optional"] = true, ["hint"] = hint };
	}

	private static JObject Flag(string name)
	{
		return new JObject { ["name"] = name, ["kind"] = "bool" };
	}

	private static JObject Of(string name, string kind)
	{
		return new JObject { ["name"] = name, ["kind"] = kind };
	}

	private static bool InLobby => StartOfRound.Instance != null && LethalMenu.localPlayer != null;

	private static void NeedLobby()
	{
		if (!InLobby)
		{
			throw new InvalidOperationException("Join or host a lobby first");
		}
	}

	private static void NeedHost()
	{
		NeedLobby();
		if (!PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			throw new InvalidOperationException("Only the host can do this");
		}
	}

	private static EnemyAI EnemyArg(JToken token)
	{
		int id = (int)token;
		return LethalMenu.enemies.FirstOrDefault((EnemyAI e) => e != null && e.GetInstanceID() == id) ?? throw new InvalidOperationException("Enemy not found (it may have despawned)");
	}

	private static PlayerControllerB PlayerArg(JToken token)
	{
		int id = (int)token;
		PlayerControllerB[] players = StartOfRound.Instance?.allPlayerScripts;
		if (players == null || id < 0 || id >= players.Length || players[id] == null)
		{
			throw new InvalidOperationException("Player not found");
		}
		return players[id];
	}

	private static string Text(JArray args, int index)
	{
		JToken token = index < args.Count ? args[index] : null;
		string value = token == null || token.Type == JTokenType.Null ? null : (string)token;
		return !string.IsNullOrEmpty(value) ? value : throw new InvalidOperationException("Pick something from the list first");
	}

	private static int Int(JArray args, int index, int fallback)
	{
		JToken token = index < args.Count ? args[index] : null;
		return token == null || token.Type == JTokenType.Null ? fallback : (int)token;
	}

	// ---- lists ----

	private static List<EnemyType> EnemyTypes => InLobby ? GameUtil.GetEnemyTypes().Where((EnemyType e) => e != null).ToList() : new List<EnemyType>();

	private static List<SelectableLevel> Moons => InLobby && StartOfRound.Instance.levels != null
		? StartOfRound.Instance.levels.Where((SelectableLevel s) => s != null && SceneUtility.GetBuildIndexByScenePath(s.sceneName) != -1).ToList()
		: new List<SelectableLevel>();

	private static List<Item> Items => InLobby && StartOfRound.Instance.allItemsList != null
		? StartOfRound.Instance.allItemsList.itemsList.Where((Item i) => i != null && i.spawnPrefab != null).ToList()
		: new List<Item>();

	private static readonly Unlockable[] Suits = new Unlockable[7]
	{
		Unlockable.OrangeSuit, Unlockable.GreenSuit, Unlockable.HazardSuit, Unlockable.PajamaSuit, Unlockable.PurpleSuit, Unlockable.BeeSuit, Unlockable.BunnySuit
	};

	private static string UnlockableName(Unlockable unlockable)
	{
		List<UnlockableItem> list = StartOfRound.Instance?.unlockablesList?.unlockables;
		int index = (int)unlockable;
		return list != null && index >= 0 && index < list.Count && list[index] != null ? list[index].unlockableName : null;
	}

	private static IEnumerable<Unlockable> Unlockables => InLobby ? Enum.GetValues(typeof(Unlockable)).Cast<Unlockable>().Where((Unlockable u) => UnlockableName(u) != null) : Enumerable.Empty<Unlockable>();

	private static void Buy(Unlockable unlockable)
	{
		if (LethalMenu.terminal == null)
		{
			throw new InvalidOperationException("The ship terminal isn't loaded yet");
		}
		unlockable.Buy(LethalMenu.terminal.groupCredits);
	}

	// ---- the actions ----

	private static readonly List<Extra> All = new List<Extra>
	{
		new Extra
		{
			Key = "SpawnEnemy", Name = "Spawn Enemy (host)", Tab = "Enemies", Group = "Enemy spawner",
			Args = () => new JArray(Choice("Enemy", EnemyTypes.Select((EnemyType e) => EnemyAIExtensions.ShownName(e.name))), Number("Amount", "1"), Flag("Outside")),
			Run = (JArray a) =>
			{
				NeedHost();
				if (StartOfRound.Instance.inShipPhase)
				{
					throw new InvalidOperationException("Land on a moon first");
				}
				string wanted = Text(a, 0);
				EnemyType type = EnemyTypes.FirstOrDefault((EnemyType e) => EnemyAIExtensions.ShownName(e.name) == wanted) ?? throw new InvalidOperationException("Unknown enemy '" + wanted + "'");
				int amount = Mathf.Clamp(Int(a, 1, 1), 1, 50);
				bool outside = a.Count > 2 && a[2].Type == JTokenType.Boolean && (bool)a[2];
				HackExecutor.SpawnEnemy(type, amount, outside);
				return "Spawned " + amount + " " + wanted + (outside ? " outside" : " inside");
			}
		},
		new Extra
		{
			Key = "KillEnemy", Name = "Kill Enemy", Tab = "Enemies", Group = "Enemies", Args = () => new JArray(Of("Enemy", "enemy")),
			Run = (JArray a) => { EnemyAI e = EnemyArg(a[0]); string n = e.ShownName(); e.Handle().Kill(); return "Killed " + n; }
		},
		new Extra
		{
			Key = "StunEnemy", Name = "Stun Enemy", Tab = "Enemies", Group = "Enemies", Args = () => new JArray(Of("Enemy", "enemy")),
			Run = (JArray a) => { EnemyAI e = EnemyArg(a[0]); e.Handle().Stun(); return "Stunned " + e.ShownName(); }
		},
		new Extra
		{
			Key = "StealItems", Name = "Steal Items Enemy", Tab = "Enemies", Group = "Enemies", Only = nameof(HoarderBugAI), Args = () => new JArray(Of("Enemy", "enemy")),
			Run = (JArray a) =>
			{
				HoarderBugAI bug = EnemyArg(a[0]) as HoarderBugAI ?? throw new InvalidOperationException("Only Hoarding Bugs steal items");
				bug.StealAllItems();
				return "Hoarding Bug is stealing every item";
			}
		},
		new Extra
		{
			Key = "SpawnWeb", Name = "Spawn Web Enemy", Tab = "Enemies", Group = "Enemies", Only = nameof(SandSpiderAI), Args = () => new JArray(Of("Enemy", "enemy")),
			Run = (JArray a) =>
			{
				SandSpiderAI spider = EnemyArg(a[0]) as SandSpiderAI ?? throw new InvalidOperationException("Only Bunker Spiders make webs");
				spider.SpawnWeb(spider.abdomen.position);
				return "Bunker Spider placed a web";
			}
		},
		new Extra
		{
			Key = "TargetPlayer", Name = "Target Player (anger at)", Tab = "Enemies", Group = "Enemies", Args = () => new JArray(Of("Enemy", "enemy"), Of("Player", "player")),
			Run = (JArray a) => { EnemyAI e = EnemyArg(a[0]); PlayerControllerB p = PlayerArg(a[1]); e.Handle().TargetPlayer(p); return e.ShownName() + " is now after " + p.playerUsername; }
		},
		new Extra
		{
			Key = "KillPlayerWithEnemy", Name = "Kill Player With Enemy", Tab = "Enemies", Group = "Enemies", Not = Cheats.EnemyHunt.Harmless, Args = () => new JArray(Of("Enemy", "enemy"), Of("Player", "player")),
			// Not an instant kill: the monster hunts the player down and attacks with its own normal damage,
			// and keeps at it until they are dead (or Stop is pressed).
			Run = (JArray a) => Cheats.EnemyHunt.Begin(EnemyArg(a[0]), PlayerArg(a[1]), kill: true)
		},
		new Extra
		{
			Key = "KillEveryone", Name = "Kill Everyone", Tab = "Players", Group = "Player actions",
			Run = (JArray a) => { NeedLobby(); LethalMenu.players.Where((PlayerControllerB p) => p != null).ToList().ForEach((PlayerControllerB p) => Hack.KillPlayer.Execute(p)); return "Killed everyone"; }
		},
		new Extra
		{
			Key = "KillEveryoneElse", Name = "Kill Everyone Else", Tab = "Players", Group = "Player actions",
			Run = (JArray a) =>
			{
				NeedLobby();
				LethalMenu.players.Where((PlayerControllerB p) => p != null && p != LethalMenu.localPlayer).ToList().ForEach((PlayerControllerB p) => Hack.KillPlayer.Execute(p));
				return "Killed everyone else";
			}
		},
		new Extra
		{
			Key = "ChangeMoon", Name = "Change Moon", Tab = "Server", Group = "Moons & Weather",
			Args = () => new JArray(Choice("Moon", Moons.Select((SelectableLevel m) => m.PlanetName))),
			Run = (JArray a) =>
			{
				NeedLobby();
				string wanted = Text(a, 0);
				SelectableLevel moon = Moons.FirstOrDefault((SelectableLevel m) => m.PlanetName == wanted) ?? throw new InvalidOperationException("Unknown moon '" + wanted + "'");
				RoundHandler.ChangeMoon(moon.levelID);
				return "Changed moon to " + wanted;
			}
		},
		new Extra
		{
			Key = "SetWeather", Name = "Set Weather (host)", Tab = "Server", Group = "Moons & Weather",
			Args = () => new JArray(Choice("Weather", Enum.GetNames(typeof(LevelWeatherType)))),
			Run = (JArray a) =>
			{
				NeedHost();
				LevelWeatherType weather = (LevelWeatherType)Enum.Parse(typeof(LevelWeatherType), Text(a, 0), ignoreCase: true);
				StartOfRound.Instance.currentLevel.currentWeather = weather;
				return StartOfRound.Instance.currentLevel.PlanetName + " weather set to " + weather;
			}
		},
		new Extra
		{
			Key = "Unlock", Name = "Unlock Ship Item", Tab = "Server", Group = "Unlocks",
			Args = () => new JArray(Choice("Item", Unlockables.Where((Unlockable u) => !Suits.Contains(u)).Select(UnlockableName))),
			Run = (JArray a) =>
			{
				NeedLobby();
				string wanted = Text(a, 0);
				Unlockable found = Unlockables.Where((Unlockable u) => UnlockableName(u) == wanted).DefaultIfEmpty((Unlockable)(-1)).First();
				if ((int)found < 0)
				{
					throw new InvalidOperationException("Unknown item '" + wanted + "'");
				}
				Buy(found);
				return "Unlocked " + wanted;
			}
		},
		new Extra
		{
			Key = "UnlockAll", Name = "Unlock All Ship Items", Tab = "Server", Group = "Unlocks",
			Run = (JArray a) => { NeedLobby(); foreach (Unlockable u in Unlockables.ToList()) { Buy(u); } return "Unlocked all ship items"; }
		},
		new Extra
		{
			Key = "UnlockSuit", Name = "Unlock Suit", Tab = "Server", Group = "Unlocks",
			Args = () => new JArray(Choice("Suit", InLobby ? Suits.Select(UnlockableName) : Enumerable.Empty<string>())),
			Run = (JArray a) =>
			{
				NeedLobby();
				string wanted = Text(a, 0);
				foreach (Unlockable suit in Suits)
				{
					if (UnlockableName(suit) == wanted)
					{
						Buy(suit);
						suit.SetLocked(locked: true);
						return "Unlocked " + wanted;
					}
				}
				throw new InvalidOperationException("Unknown suit '" + wanted + "'");
			}
		},
		new Extra
		{
			Key = "UnlockAllSuits", Name = "Unlock All Suits", Tab = "Server", Group = "Unlocks",
			Run = (JArray a) => { NeedLobby(); foreach (Unlockable suit in Suits) { if (UnlockableName(suit) != null) { Buy(suit); suit.SetLocked(locked: true); } } return "Unlocked all suits"; }
		},
		new Extra
		{
			Key = "SpawnItem", Name = "Spawn Item (host)", Tab = "Server", Group = "Spawning (host)",
			Args = () => new JArray(Choice("Item", Items.Select((Item i) => i.name)), Number("Amount", "1"), Number("Scrap Value", "random")),
			Run = (JArray a) =>
			{
				NeedHost();
				string wanted = Text(a, 0);
				Item item = Items.FirstOrDefault((Item i) => i.name == wanted) ?? throw new InvalidOperationException("Unknown item '" + wanted + "'");
				int amount = Mathf.Clamp(Int(a, 1, 1), 1, 50);
				int value = Int(a, 2, -1);
				if (value < 0)
				{
					value = UnityEngine.Random.Range(15, 100);
				}
				for (int i = 0; i < amount; i++)
				{
					GameObject spawned = UnityEngine.Object.Instantiate(item.spawnPrefab, LethalMenu.localPlayer.playerEye.transform.position, Quaternion.identity, StartOfRound.Instance.propsContainer);
					GrabbableObject grabbable = spawned.GetComponent<GrabbableObject>();
					grabbable.SetScrapValue(value);
					grabbable.fallTime = 0f;
					spawned.GetComponent<NetworkObject>().Spawn();
				}
				return "Spawned " + amount + " " + item.itemName + " (worth " + value + ") at your feet";
			}
		}
	};

	/// <summary>Adds the extra actions to the list of features sent to the website.</summary>
	public static void AddToCatalog(JObject hacks)
	{
		int order = 20000;
		foreach (Extra extra in All)
		{
			JObject entry = new JObject
			{
				["name"] = extra.Name,
				["category"] = extra.Tab,
				["group"] = extra.Group,
				["order"] = order++,
				["toggle"] = false,
				["action"] = true,
				["args"] = extra.Args()
			};
			if (extra.Only != null)
			{
				entry["only"] = extra.Only;
			}
			if (extra.Not != null)
			{
				entry["not"] = new JArray(extra.Not);
			}
			hacks[Prefix + extra.Key] = entry;
		}
	}

	public static bool IsExtra(string name)
	{
		return name != null && name.StartsWith(Prefix, StringComparison.Ordinal);
	}

	public static string Run(string name, JArray args)
	{
		string key = name.Substring(Prefix.Length);
		Extra extra = All.FirstOrDefault((Extra e) => e.Key == key) ?? throw new InvalidOperationException("Unknown action '" + name + "'");
		return extra.Run(args ?? new JArray());
	}
}
