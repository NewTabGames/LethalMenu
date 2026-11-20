using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GameNetcodeStuff;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LethalMenu.Remote;

/// <summary>
/// Describes the menu to the website (which hacks exist, how to call them, which settings exist) and
/// builds the live state snapshot (toggles, settings, players, enemies, game info).
/// </summary>
internal static class RemoteCatalog
{
	/// <summary>Hacks that make no sense from a browser.</summary>
	private static readonly HashSet<Hack> Hidden = new HashSet<Hack>
	{
		Hack.OpenMenu,
		Hack.ToggleCursor,
		Hack.UnloadMenu,
		Hack.ClickTeleportAction,
		Hack.ClickKillAction,
		Hack.UnlockObjectsAction,
		Hack.DeathNotify,
		Hack.EnemyDeathNotify
	};

	/// <summary>Same tabs and sections as the in-game menu (keep the two in sync).</summary>
	private static readonly (string Tab, string Group, string Hacks)[] Layout = new (string, string, string)[]
	{
		("Self", "Survival", "GodMode GhostMode Invisibility NoFallDamage UnlimitedOxygen NoQuicksand AntiGhostGirl"),
		("Self", "Movement", "SuperJump FastClimb SuperSpeed UnlimitedStamina NoClip FreeCam ClickTeleport TeleportWithItems Teleport TeleportShip SaveTeleportPosition TeleportSavedPosition"),
		("Self", "Interaction", "Reach InteractThroughWalls LootThroughWalls LootThroughWallsBeltBag LootAnyItemBeltBag LootBeforeGameStarts NoCooldown InstantInteract UnlockObjects BuildAnywhere"),
		("Self", "Items & Inventory", "ItemSlots Weight StrongHands UnlimitedBattery UnlimitedPresents UnlimitedTZP NoTZPEffects DropAllItems DeleteHeldItem TeleportAllItems TeleportOneItem SellEverything SellQuota Experience"),
		("Self", "Weapons", "ClickKill SuperShovel SuperKnife UnlimitedAmmo MinigunShotgun GrabNutcrackerShotgun UnlimitedZapGun EggsNeverExplode EggsAlwaysExplode"),
		("Self", "Voice Chat", "HearAllAlivePeople HearAllDeadPeople"),
		("Self", "Facility & Ship", "UnlockAllDoors OpenAllBigDoors CloseAllBigDoors FixAllValves BridgeNeverFalls NoShipDoorClose OpenDropShipLand VoteShipLeaveEarly VehicleGodMode"),
		("Players", "Player actions", "DeathNotifications SpectatePlayer MiniCam HealPlayer DemiGod KillPlayer LightningStrikePlayer SpiderWebPlayer ExplodeClosestMine LureAllEnemies TeleportAllEnemies ExplodeJetpack"),
		("Enemies", "Enemies", "EnemyControl HuntPlayer PersistentHunt AggroHunt HuntWithAll StopAllHunts TeleportEnemy KillNearbyEnemies KillAllEnemies StunAllEnemies BreakAllWebs EnemyDeathNotifications"),
		("Visuals", "ESP style", "ToggleAllESP NameESP BoxESP"),
		("Visuals", "People & Creatures", "PlayerESP BodyESP EnemyESP EnemyVentESP"),
		("Visuals", "Loot", "ObjectESP ItemDropShipESP"),
		("Visuals", "Hazards", "LandmineESP TurretESP SpikeRoofTrapESP SteamHazardESP VainShroudESP"),
		("Visuals", "Places", "ShipESP DoorESP BigDoorESP DoorLockESP BreakerESP MineshaftElevatorESP CruiserESP"),
		("Visuals", "HUD", "AlwaysShowClock SimpleClock Crosshair HPDisplay Breadcrumbs"),
		("Visuals", "Camera & Screen", "FOV NightVision NoVisor NoFieldOfDepth NoFlash NoCameraShake FullRenderResolution"),
		("Server", "Money & Quota", "ModifyCredits ModifyQuota ModifyDeadline ModifyScrap Shoplifter"),
		("Server", "Round", "StartGame EndGame ResetShip ForceMeteorShower ClearMeteorShower"),
		("Server", "Spawning (host)", "SpawnMoreScrap SpawnMapObjects SpawnLandmine SpawnTurret SpawnSpikeRoofTrap"),
		("Server", "Chat", "Message"),
		("Server", "Lobby", "JoinLobby Disconnect ShowOffensiveLobbyNames"),
		("Server", "Info display", "ToggleAllDisplays DisplayBodies DisplayEnemies DisplayMapObjects DisplayShipObjects DisplayQuota DisplayDeadline DisplayCredits"),
		("Troll", "Sounds", "ToggleShipHorn ToggleCarHorn ToggleTerminalSound ToggleDepositDeskDoorSound PJSpammer SlideTaunt"),
		("Troll", "Ship & Facility", "ToggleShipLights ToggleFactoryLights OpenShipDoorSpace EjectEveryone ToggleMineshaftElevator"),
		("Troll", "Traps & Explosives", "BlowUpAllLandmines ToggleAllLandmines ToggleAllTurrets BerserkAllTurrets ForceBridgeFall ForceSmallBridgeFall ExplodeCruiser ExplodeAllJetpacks ExplodeJetpacksOnGrab ToggleVehicleMagnet"),
		("Troll", "Shotguns", "ShootAllShotguns SpamShootAllShotguns"),
		("Troll", "Monsters", "ForceTentacleAttack SpawnMaskedEnemy SpawnHoardingBugInfestation")
	};

	/// <summary>
	/// Features that have a slider next to them in the in-game menu (keep in sync with UI.HackSlider calls).
	/// The website shows the same slider under the feature. Min/max are read when the list is sent, since some
	/// start at the game's own default value.
	/// </summary>
	private static IEnumerable<(Hack Hack, string Setting, float Min, float Max, float Step)> Sliders()
	{
		yield return (Hack.FOV, "f_fov", 10f, 180f, 1f);
		yield return (Hack.SuperJump, "f_jumpForce", Settings.f_defaultJumpForce, Settings.f_defaultJumpForce + 200f, 0.5f);
		yield return (Hack.FastClimb, "f_climbSpeed", Settings.f_defaultClimbSpeed, Settings.f_defaultClimbSpeed + 20f, 0.5f);
		yield return (Hack.SuperSpeed, "f_movementSpeed", Settings.f_defaultMovementSpeed, Settings.f_defaultMovementSpeed + 20f, 0.5f);
		yield return (Hack.NoClip, "f_noclipSpeed", 10f, 30f, 0.5f);
		yield return (Hack.ItemSlots, "f_slots", 1f, 20f, 1f);
		yield return (Hack.KillNearbyEnemies, "f_enemyKillDistance", 5f, 50f, 1f);
		yield return (Hack.PJSpammer, "f_pjSpamSpeed", 0f, 1f, 0.05f);
		yield return (Hack.BoxESP, "f_ESPThickness", 1f, 10f, 1f);
	}

	/// <summary>Settings the website should not touch (window geometry, captured game defaults).</summary>
	private static readonly HashSet<string> HiddenSettings = new HashSet<string> { "i_menuWidth", "i_menuHeight" };

	private static Dictionary<Hack, (string Tab, string Group, int Order)> placements;

	private static (string Tab, string Group, int Order) PlacementOf(Hack hack)
	{
		if (placements == null)
		{
			placements = new Dictionary<Hack, (string, string, int)>();
			int order = 0;
			foreach (var (tab, group, hacks) in Layout)
			{
				foreach (string name in hacks.Split(' '))
				{
					if (!Enum.TryParse<Hack>(name, out var h))
					{
						// A typo here sends the feature to an "Other" tab on the website.
						Debug.LogWarning("[LethalMenu] Remote layout: no feature called '" + name + "'");
					}
					else if (!placements.ContainsKey(h))
					{
						placements.Add(h, (tab, group, order++));
					}
				}
			}
		}
		return placements.TryGetValue(hack, out var placement) ? placement : ("Other", "Other", 10000);
	}

	internal static string Humanize(string name)
	{
		name = Regex.Replace(name, "^(f|i|b)_", "");
		name = Regex.Replace(name, "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
		return char.ToUpperInvariant(name[0]) + name.Substring(1);
	}

	/// <summary>Kind of argument the website has to provide for a parameter type, or null if it cannot.</summary>
	internal static string ArgKind(Type type)
	{
		if (type == typeof(PlayerControllerB))
		{
			return "player";
		}
		if (type == typeof(EnemyAI))
		{
			return "enemy";
		}
		if (type == typeof(EnemyAI[]))
		{
			return "allEnemies";
		}
		if (type == typeof(int) || type == typeof(float))
		{
			return "number";
		}
		if (type == typeof(string))
		{
			return "text";
		}
		if (type == typeof(bool))
		{
			return "bool";
		}
		if (type == typeof(Vector3))
		{
			return "vector3";
		}
		if (type.IsEnum)
		{
			return "enum";
		}
		return null;
	}

	internal static IEnumerable<FieldInfo> SettingFields()
	{
		return from f in typeof(Settings).GetFields(BindingFlags.Static | BindingFlags.Public)
			where (f.FieldType == typeof(float) || f.FieldType == typeof(int) || f.FieldType == typeof(bool))
				&& Regex.IsMatch(f.Name, "^(f|i|b)_") && !f.Name.StartsWith("f_default") && !HiddenSettings.Contains(f.Name)
			select f;
	}

	/// <param name="live">True on the main thread: also lists the in-game tools (spawners, managers) with their
	/// current choices. False from the network thread, which must not touch game objects.</param>
	public static JObject BuildCatalog(bool live = false)
	{
		JObject hacks = new JObject();
		foreach (Hack hack in Enum.GetValues(typeof(Hack)))
		{
			if (Hidden.Contains(hack))
			{
				continue;
			}
			bool toggle = hack.CanBeToggled();
			Delegate executor = hack.GetExecutor();
			ParameterInfo[] parameters = executor?.Method.GetParameters() ?? new ParameterInfo[0];
			JArray args = new JArray();
			bool remoteCallable = true;
			foreach (ParameterInfo parameter in parameters)
			{
				string kind = ArgKind(parameter.ParameterType);
				if (kind == null)
				{
					remoteCallable = false;
					break;
				}
				JObject arg = new JObject
				{
					["name"] = Humanize(parameter.Name),
					["kind"] = kind
				};
				if (kind == "enum")
				{
					arg["options"] = new JArray(Enum.GetNames(parameter.ParameterType));
				}
				if (kind == "number")
				{
					arg["integer"] = parameter.ParameterType == typeof(int);
				}
				args.Add(arg);
			}
			if (!toggle && (executor == null || !remoteCallable))
			{
				continue;
			}
			hacks[hack.ToString()] = new JObject
			{
				["name"] = Humanize(hack.ToString()),
				["category"] = PlacementOf(hack).Tab,
				["group"] = PlacementOf(hack).Group,
				["order"] = PlacementOf(hack).Order,
				["toggle"] = toggle,
				// A toggle whose executor needs arguments (e.g. Spectate Player) is switched on through "run".
				["action"] = executor != null && remoteCallable && (!toggle || args.Count > 0),
				["args"] = args
			};
		}
		foreach (var (hack, setting, min, max, step) in Sliders())
		{
			if (hacks[hack.ToString()] is JObject entry)
			{
				entry["slider"] = new JObject
				{
					["setting"] = setting,
					["min"] = Math.Round(min, 3),
					["max"] = Math.Round(max, 3),
					["step"] = step
				};
			}
		}
		// Aggro Hunt only fits Eyeless Dogs: the website shows it on their cards only.
		if (hacks[Hack.AggroHunt.ToString()] is JObject aggro)
		{
			aggro["only"] = nameof(MouthDogAI);
		}
		if (live)
		{
			RemoteExtras.AddToCatalog(hacks);
		}
		JObject settings = new JObject();
		foreach (FieldInfo field in SettingFields())
		{
			settings[field.Name] = new JObject
			{
				["name"] = Humanize(field.Name),
				["type"] = field.FieldType == typeof(bool) ? "bool" : (field.FieldType == typeof(int) ? "int" : "float")
			};
		}
		return new JObject
		{
			["version"] = Settings.version,
			["hacks"] = hacks,
			["settings"] = settings
		};
	}

	/// <summary>Map position for the website: [x, z] to the nearest half metre (seen from above).</summary>
	private static JArray MapPos(Vector3 p)
	{
		// x and z place it on the map; the height (y) tells floors apart inside the building, which the map flattens
		// onto one picture. Older websites just ignore the third number.
		return new JArray(Math.Round(p.x * 2) / 2, Math.Round(p.z * 2) / 2, Math.Round(p.y));
	}

	private static readonly HarmonyLib.AccessTools.FieldRef<Landmine, bool> MineActivated = HarmonyLib.AccessTools.FieldRefAccess<Landmine, bool>("mineActivated");

	private static JObject Hazard(string type, Vector3 position, bool on)
	{
		return new JObject
		{
			["t"] = type,
			["on"] = on,
			["i"] = position.y < -80f,
			["p"] = MapPos(position)
		};
	}

	/// <summary>Live snapshot; must be called on the main thread.</summary>
	public static JObject BuildState()
	{
		JObject toggles = new JObject();
		foreach (KeyValuePair<Hack, bool> pair in HackExtensions.ToggleFlags)
		{
			if (!Hidden.Contains(pair.Key))
			{
				toggles[pair.Key.ToString()] = pair.Value;
			}
		}
		JObject settings = new JObject();
		foreach (FieldInfo field in SettingFields())
		{
			object value = field.GetValue(null);
			settings[field.Name] = value is float f ? (JToken)Math.Round(f, 3) : JToken.FromObject(value);
		}
		JArray players = new JArray();
		JArray enemies = new JArray();
		JObject game = new JObject
		{
			["inRound"] = false
		};
		StartOfRound round = StartOfRound.Instance;
		PlayerControllerB local = GameNetworkManager.Instance?.localPlayerController;
		// Positions only matter to the map; they change all the time, so they're only sent while it's open.
		bool map = RemoteControl.MapWatched;
		JArray items = new JArray();
		JArray hazards = new JArray();
		JArray dormant = new JArray();
		if (round != null)
		{
			game["inRound"] = true;
			game["landed"] = round.shipHasLanded;
			game["inShipPhase"] = round.inShipPhase;
			game["moon"] = round.currentLevel?.PlanetName ?? "";
			game["isHost"] = local != null && local.IsHost;
			game["credits"] = LethalMenu.terminal != null ? LethalMenu.terminal.groupCredits : 0;
			if (map && round.elevatorTransform != null)
			{
				game["ship"] = MapPos(round.elevatorTransform.position);
			}
			// Which map the website should show (RemoteMap sends it once per landing).
			game["mapSeed"] = round.randomMapSeed;
			game["mapReady"] = RemoteMap.ReadyForCurrent();
			if (TimeOfDay.Instance != null)
			{
				game["quota"] = TimeOfDay.Instance.profitQuota;
				game["fulfilled"] = TimeOfDay.Instance.quotaFulfilled;
				game["daysLeft"] = TimeOfDay.Instance.daysUntilDeadline;
			}
			if (local != null)
			{
				game["localName"] = local.playerUsername;
				game["alive"] = !local.isPlayerDead;
				game["health"] = local.health;
				Vector3 p = local.transform.position;
				game["position"] = new JArray(Math.Round(p.x, 2), Math.Round(p.y, 2), Math.Round(p.z, 2));
			}
			PlayerControllerB[] all = round.allPlayerScripts ?? new PlayerControllerB[0];
			for (int i = 0; i < all.Length; i++)
			{
				PlayerControllerB player = all[i];
				if (player != null && (player.isPlayerControlled || player.isPlayerDead))
				{
					players.Add(new JObject
					{
						["id"] = i,
						["clientId"] = (int)player.playerClientId,
						["name"] = player.playerUsername,
						["dead"] = player.isPlayerDead,
						["health"] = player.health,
						["local"] = player == local,
						["inside"] = player.isInsideFactory,
						["item"] = player.currentlyHeldObjectServer != null && player.currentlyHeldObjectServer.itemProperties != null ? player.currentlyHeldObjectServer.itemProperties.itemName : null,
						["pos"] = map ? MapPos(player.isPlayerDead && player.deadBody != null ? player.deadBody.transform.position : player.transform.position) : null,
						["route"] = map ? RemoteRoutes.For(player, i) : null
					});
				}
			}
			foreach (EnemyAI enemy in LethalMenu.enemies)
			{
				if (enemy != null)
				{
					enemies.Add(new JObject
					{
						["id"] = enemy.GetInstanceID(),
						["name"] = Handler.EnemyAIExtensions.ShownName(enemy),
						["type"] = enemy.GetType().Name,
						["dead"] = enemy.isEnemyDead,
						["inside"] = !Handler.EnemyAIExtensions.IsOutsideNow(enemy),
						["distance"] = local != null ? Math.Round(Vector3.Distance(local.transform.position, enemy.transform.position)) : 0.0,
						["pos"] = map ? MapPos(enemy.transform.position) : null
					});
				}
			}
			if (map)
			{
				// Items lying around (not held, not in the ship), for the map's "Items" layer. Items the game has
				// switched off or tucked into someone's pockets aren't really lying anywhere, so they're left out.
				foreach (GrabbableObject item in LethalMenu.items)
				{
					if (item == null || item.isHeld || item.isHeldByEnemy || item.isPocketed || item.deactivated || item.isInShipRoom
						|| !item.gameObject.activeInHierarchy || item.itemProperties == null || items.Count >= 300)
					{
						continue;
					}
					items.Add(new JObject
					{
						["n"] = item.itemProperties.itemName,
						["v"] = item.scrapValue,
						["i"] = item.isInFactory,
						["p"] = MapPos(item.transform.position)
					});
				}
				// Traps for the map: turrets, landmines (not once they've gone off) and spike traps, and whether each
				// is switched on (a terminal can switch them off for a while). Inside means below the surface, like
				// the monsters.
				foreach (Turret turret in LethalMenu.turrets)
				{
					if (turret != null)
					{
						hazards.Add(Hazard("turret", turret.transform.position, turret.turretActive));
					}
				}
				foreach (Landmine mine in LethalMenu.landmines)
				{
					if (mine != null && !mine.hasExploded)
					{
						hazards.Add(Hazard("mine", mine.transform.position, MineActivated(mine)));
					}
				}
				foreach (SpikeRoofTrap spikes in LethalMenu.spikeRoofTraps)
				{
					if (spikes != null)
					{
						hazards.Add(Hazard("spikes", spikes.transform.position, spikes.trapActive));
					}
				}
				// Dormant monsters: an Old Bird sitting in its nest isn't a monster yet (just the nest), so it wasn't in
				// the monster list or on the map until it woke up.
				if (RoundManager.Instance != null)
				{
					foreach (EnemyAINestSpawnObject nest in RoundManager.Instance.enemyNestSpawnObjects)
					{
						if (nest != null && nest.enemyType != null)
						{
							dormant.Add(new JObject
							{
								["n"] = Handler.EnemyAIExtensions.ShownName(nest.enemyType.enemyName),
								["i"] = nest.transform.position.y < -80f,
								["p"] = MapPos(nest.transform.position)
							});
						}
					}
				}
			}
		}
		return new JObject
		{
			["toggles"] = toggles,
			["settings"] = settings,
			["game"] = game,
			["players"] = players,
			["enemies"] = enemies,
			["items"] = items,
			["hazards"] = hazards,
			["dormant"] = dormant,
			// Who Spectate / Mini Cam / Demi God currently point at (player clientIds), so the website can offer "Stop".
			["targets"] = new JObject
			{
				["spectating"] = Hack.SpectatePlayer.IsEnabled() ? Cheats.SpectatePlayer.spectatingPlayer : -1,
				["miniCam"] = Hack.MiniCam.IsEnabled() ? Cheats.SpectatePlayer.camPlayer : -1,
				["controlTarget"] = Cheats.EnemyControl.ValidChosenTarget != null ? (int)Cheats.EnemyControl.ValidChosenTarget.playerClientId : -1,
				["aiDriving"] = Cheats.EnemyControl.ControlledEnemy != null && Cheats.EnemyControl.IsAIControlled,
				["controlling"] = Cheats.EnemyControl.ControlledEnemy != null ? Handler.EnemyAIExtensions.ShownName(Cheats.EnemyControl.ControlledEnemy) : null,
				["hunts"] = new JArray(Cheats.EnemyHunt.Hunts.Where((Cheats.EnemyHunt.Hunt h) => h.Enemy != null && h.Target != null).Select((Cheats.EnemyHunt.Hunt h) => new JObject
				{
					["enemy"] = h.Enemy.GetInstanceID(),
					["enemyName"] = Cheats.EnemyHunt.EnemyName(h.Enemy),
					["persistent"] = h.Persistent,
					["aggro"] = h.Aggro,
					["player"] = h.Target.playerUsername
				})),
				["demiGod"] = new JArray(Cheats.DemiGodCheat.DemiGodPlayers.Where((PlayerControllerB p) => p != null).Select((PlayerControllerB p) => (int)p.playerClientId))
			},
			["menuOpen"] = Settings.isMenuOpen
		};
	}
}
