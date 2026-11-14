using System;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LethalMenu.Remote;

/// <summary>Runs a command sent from the website. Main thread only. Returns a short result message, throws on failure.</summary>
internal static class RemoteCommands
{
	public static string Execute(JObject command)
	{
		string type = (string)command["type"];
		if (VersionLock.Locked && type != "ping")
		{
			throw new InvalidOperationException(VersionLock.Message + " " + VersionLock.RepoUrl);
		}
		switch (type)
		{
		case "toggle":
			return Toggle(ParseHack(command), (bool)command["value"]);
		case "run":
			if (RemoteExtras.IsExtra((string)command["hack"]))
			{
				return RemoteExtras.Run((string)command["hack"], command["args"] as JArray);
			}
			return Run(ParseHack(command), command["args"] as JArray ?? new JArray());
		case "setting":
			return SetSetting((string)command["name"], command["value"]);
		case "menu":
		{
			bool open = (bool)command["open"];
			if (open && Settings.b_StealthMode)
			{
				throw new InvalidOperationException("Stealth Mode is on: the in-game menu stays hidden. Turn Stealth off first");
			}
			if (Settings.isMenuOpen != open)
			{
				Hack.OpenMenu.Execute();
			}
			return open ? "Menu opened" : "Menu closed";
		}
		case "controlTarget":
		{
			JToken player = command["player"];
			if (player == null || player.Type == JTokenType.Null || (int)player < 0)
			{
				EnemyControl.ChosenTarget = null;
				return "Target: nearest player";
			}
			EnemyControl.ChosenTarget = (PlayerControllerB)ResolveArg(typeof(PlayerControllerB), player);
			return "Target: " + EnemyControl.ChosenTarget.playerUsername;
		}
		case "controlAI":
		{
			if (EnemyControl.ControlledEnemy == null)
			{
				throw new InvalidOperationException("Not controlling an enemy");
			}
			EnemyControl.SetAIDriving((bool)command["value"]);
			return EnemyControl.IsAIControlled ? "AI is driving" : "You are driving";
		}
		case "stopHunt":
		{
			EnemyAI hunter = (EnemyAI)ResolveArg(typeof(EnemyAI), command["enemy"]);
			return EnemyHunt.Stop(hunter) ? EnemyHunt.EnemyName(hunter) + " stopped hunting" : "It wasn't hunting";
		}
		case "saveSettings":
			Settings.Config.SaveConfig();
			return "Settings saved";
		case "eject":
			Ejector.Eject();
			return "Ejecting";
		case "ping":
			// The website pings with map = true every so often while its Map tab is open, and map = false when
			// it leaves. Map dots (positions, items) are only sent while someone is looking at the map.
			if (command["map"] != null && command["map"].Type == JTokenType.Boolean)
			{
				RemoteControl.MapWatchedUntil = (bool)command["map"] ? Time.unscaledTime + 45f : 0f;
			}
			return "pong";
		default:
			throw new InvalidOperationException("Unknown command '" + type + "'");
		}
	}

	private static Hack ParseHack(JObject command)
	{
		string name = (string)command["hack"];
		if (!Enum.TryParse<Hack>(name, out var hack))
		{
			throw new InvalidOperationException("Unknown hack '" + name + "'");
		}
		return hack;
	}

	private static string Toggle(Hack hack, bool value)
	{
		if (!hack.CanBeToggled())
		{
			throw new InvalidOperationException(hack + " is not a toggle");
		}
		string label = RemoteCatalog.Humanize(hack.ToString());
		if (hack.IsEnabled() == value)
		{
			return label + " already " + (value ? "on" : "off");
		}
		if (hack == Hack.EnemyControl && !value)
		{
			EnemyControl.StopControl();
			return "Released enemy";
		}
		Delegate executor = hack.GetExecutor();
		if (executor == null || executor is Action)
		{
			hack.Execute();
		}
		else if (!value)
		{
			// Hacks like Spectate Player / Mini Cam turn themselves off when their toggle clears.
			hack.SetToggle(b: false);
		}
		else
		{
			throw new InvalidOperationException(label + " needs a target; use Run");
		}
		return label + " " + (hack.IsEnabled() ? "on" : "off");
	}

	private static string Run(Hack hack, JArray args)
	{
		Delegate executor = hack.GetExecutor() ?? throw new InvalidOperationException(hack + " cannot be run");
		if (hack == Hack.LightningStrikePlayer && !Handler.PlayerHandler.CanStrike())
		{
			throw new InvalidOperationException("Lightning only works on a stormy moon after landing");
		}
		if (hack == Hack.ExplodeClosestMine && !LethalMenu.landmines.Any((Landmine m) => m != null && !m.hasExploded))
		{
			throw new InvalidOperationException("There are no live landmines on this moon");
		}
		if (hack == Hack.SpiderWebPlayer && Handler.SandSpiderAIExtensions.FindSpider() == null)
		{
			throw new InvalidOperationException("Spider webs need a living Bunker Spider somewhere on the map");
		}
		ParameterInfo[] parameters = executor.Method.GetParameters();
		object[] values = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			JToken arg = i < args.Count ? args[i] : null;
			if (arg == null || arg.Type == JTokenType.Null)
			{
				if (!parameters[i].HasDefaultValue)
				{
					throw new InvalidOperationException("Missing " + RemoteCatalog.Humanize(parameters[i].Name));
				}
				values[i] = parameters[i].DefaultValue;
			}
			else
			{
				values[i] = ResolveArg(parameters[i].ParameterType, arg);
			}
		}
		if (hack.CanBeToggled() && parameters.Length > 0)
		{
			// Same as the in-game Players tab: switch on, then point it at the target.
			hack.SetToggle(b: true);
			hack.Invoke(values);
		}
		else
		{
			hack.Execute(values);
		}
		return "Ran " + RemoteCatalog.Humanize(hack.ToString());
	}

	private static object ResolveArg(Type type, JToken arg)
	{
		if (type == typeof(PlayerControllerB))
		{
			int id = (int)arg;
			PlayerControllerB[] players = StartOfRound.Instance?.allPlayerScripts;
			if (players == null || id < 0 || id >= players.Length || players[id] == null)
			{
				throw new InvalidOperationException("Player #" + id + " not found");
			}
			return players[id];
		}
		if (type == typeof(EnemyAI))
		{
			int id = (int)arg;
			return LethalMenu.enemies.FirstOrDefault((EnemyAI e) => e != null && e.GetInstanceID() == id) ?? throw new InvalidOperationException("Enemy not found (it may have despawned)");
		}
		if (type == typeof(EnemyAI[]))
		{
			return LethalMenu.enemies.Where((EnemyAI e) => e != null).ToArray();
		}
		if (type == typeof(Vector3))
		{
			JArray xyz = (JArray)arg;
			return new Vector3((float)xyz[0], (float)xyz[1], (float)xyz[2]);
		}
		if (type.IsEnum)
		{
			return Enum.Parse(type, (string)arg, ignoreCase: true);
		}
		return arg.ToObject(type);
	}

	private static string SetSetting(string name, JToken value)
	{
		FieldInfo field = RemoteCatalog.SettingFields().FirstOrDefault((FieldInfo f) => f.Name == name) ?? throw new InvalidOperationException("Unknown setting '" + name + "'");
		field.SetValue(null, value.ToObject(field.FieldType));
		if (name == "b_NoFog")
		{
			NoFog.ToggleNoFog();
		}
		return RemoteCatalog.Humanize(name) + " = " + field.GetValue(null);
	}
}
