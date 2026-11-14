using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Handler;
using LethalMenu.Manager;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

/// <summary>
/// In-game only (not on the website). Live state for finding out why something didn't work: the lobby,
/// the remote panel link, the controlled monster, hunts, every enemy, and the last errors/warnings.
/// Below that, the original developer tools.
/// </summary>
internal class DebugTab : MenuTab
{
	private Vector2 scrollPos = Vector2.zero;

	private int section;

	private static readonly string[] Sections = new string[4] { "Overview", "Enemies", "Errors", "Tools" };

	// What the tab shows is captured once per frame (on the layout pass) and reused for drawing, so the set of
	// lines and buttons can't change in between - Unity's menu system errors if it does.
	private List<string> lines = new List<string>();

	private List<DebugLog.Entry> logSnapshot = new List<DebugLog.Entry>();

	public DebugTab()
		: base("Debug")
	{
		DebugLog.Attach();
	}

	public override void Draw()
	{
		GUILayout.BeginVertical();
		if (Event.current.type == EventType.Layout)
		{
			Capture();
		}
		section = GUILayout.Toolbar(section, Sections);
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		switch (section)
		{
		case 0:
		case 1:
			DrawLines();
			break;
		case 2:
			DrawErrors();
			break;
		default:
			MenuContent();
			break;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void Capture()
	{
		try
		{
			lines = section == 0 ? Overview() : (section == 1 ? EnemyLines() : new List<string>());
		}
		catch (Exception ex)
		{
			lines = new List<string> { "Couldn't read the game state: " + ex.Message };
		}
		if (section == 2)
		{
			logSnapshot = DebugLog.Snapshot();
		}
	}

	private void DrawLines()
	{
		foreach (string line in lines)
		{
			if (line.StartsWith("# "))
			{
				GUILayout.Space(6f);
				GUILayout.Label("<b>" + line.Substring(2) + "</b>");
			}
			else
			{
				GUILayout.Label(line);
			}
		}
	}

	private static string YesNo(bool b)
	{
		return b ? "yes" : "no";
	}

	private static string Pos(Vector3 v)
	{
		return $"({v.x:0}, {v.y:0}, {v.z:0})";
	}

	private static List<string> Overview()
	{
		List<string> l = new List<string>();
		PlayerControllerB me = LethalMenu.localPlayer;
		l.Add("# Menu");
		l.Add("Version: " + Settings.version + "   |   Running under BepInEx: " + YesNo(Loader.RunningUnderBepInEx));
		l.Add("Remote panel: " + Remote.RemoteControl.StatusText);
		l.Add("Stealth Mode: " + (Settings.b_StealthMode ? "ON" : "off") + "   |   Debug Mode (game test menu): " + (Settings.DebugMode ? "on" : "off"));
		l.Add("FPS: " + LethalMenu.Instance?.fps);
		l.Add("# Game");
		if (StartOfRound.Instance == null || me == null)
		{
			l.Add("Not in a lobby.");
			return l;
		}
		StartOfRound round = StartOfRound.Instance;
		l.Add("Host: " + YesNo(PlayerHandlerExtensions.IsHost(me)) + "   |   Your client id: " + me.actualClientId + "   |   Slot: " + Array.IndexOf(round.allPlayerScripts, me));
		l.Add("Moon: " + (round.currentLevel != null ? round.currentLevel.PlanetName + " (" + round.currentLevel.currentWeather + ")" : "-") + "   |   " + (round.inShipPhase ? "In orbit" : (round.shipHasLanded ? "Landed" : "Travelling")));
		l.Add("Players: " + round.allPlayerScripts.Count((PlayerControllerB p) => p != null && p.isPlayerControlled) + " alive, " + round.allPlayerScripts.Count((PlayerControllerB p) => p != null && p.isPlayerDead) + " dead");
		l.Add("Enemies: " + LethalMenu.enemies.Count((EnemyAI e) => e != null && !e.isEnemyDead) + " alive   |   Items: " + LethalMenu.items.Count((GrabbableObject i) => i != null));
		l.Add("# You");
		l.Add("Health: " + me.health + (me.criticallyInjured ? " (critically injured)" : "") + "   |   Dead: " + YesNo(me.isPlayerDead) + "   |   Inside: " + YesNo(me.isInsideFactory) + "   |   In ship: " + YesNo(me.isInHangarShipRoom));
		l.Add("Position: " + Pos(me.transform.position));
		l.Add("Speed: " + me.movementSpeed.ToString("0.0") + "   |   Jump: " + me.jumpForce.ToString("0.0") + "   |   Climb: " + me.climbSpeed.ToString("0.0"));
		l.Add("# Enemy Control");
		EnemyAI controlled = EnemyControl.ControlledEnemy;
		if (controlled == null)
		{
			l.Add("Not controlling anything.");
		}
		else
		{
			l.Add(controlled.ShownName() + " (" + controlled.GetType().Name + ")   |   State: " + controlled.currentBehaviourStateIndex + "   |   You own it: " + YesNo(controlled.IsOwner) + " (owner " + controlled.OwnerClientId + ")");
			l.Add("AI driving: " + YesNo(EnemyControl.IsAIControlled) + "   |   Flying: " + YesNo(EnemyControl.IsFlying) + "   |   Target: " + (EnemyControl.ValidChosenTarget != null ? EnemyControl.ValidChosenTarget.playerUsername : "nearest other player"));
			l.Add("On walkable floor (nav mesh): " + YesNo(controlled.agent != null && controlled.agent.isOnNavMesh) + "   |   Position: " + Pos(controlled.transform.position));
		}
		l.Add("# Hunts");
		if (EnemyHunt.Hunts.Count == 0)
		{
			l.Add("None.");
		}
		foreach (EnemyHunt.Hunt hunt in EnemyHunt.Hunts.ToList())
		{
			if (hunt.Enemy == null || hunt.Target == null)
			{
				continue;
			}
			float walk = EnemyHunt.WalkDistance(hunt.Enemy);
			string kind = hunt.Aggro ? "Aggro" : (hunt.Persistent ? "Persistent" : "One-shot");
			l.Add(kind + ": " + hunt.Enemy.ShownName() + " -> " + hunt.Target.playerUsername
				+ "   |   " + Vector3.Distance(hunt.Enemy.transform.position, hunt.Target.transform.position).ToString("0") + "m straight, "
				+ (walk == float.MaxValue ? "no route" : walk.ToString("0") + "m walking")
				+ "   |   Sees them: " + YesNo(EnemyHunt.Sees(hunt.Enemy, hunt.Target))
				+ "   |   Chasing: " + YesNo(EnemyHunt.IsChasing(hunt.Enemy))
				+ "   |   You own it: " + YesNo(hunt.Enemy.IsOwner)
				+ "   |   " + (Time.time - hunt.StartTime).ToString("0") + "s");
		}
		return l;
	}

	private static List<string> EnemyLines()
	{
		List<string> l = new List<string>();
		PlayerControllerB me = LethalMenu.localPlayer;
		List<EnemyAI> list = LethalMenu.enemies.Where((EnemyAI e) => e != null).OrderBy((EnemyAI e) => e.isEnemyDead).ThenBy((EnemyAI e) => e.ShownName()).ToList();
		if (list.Count == 0)
		{
			l.Add("No enemies.");
			return l;
		}
		l.Add("Name (type)  |  state  |  owner  |  distance  |  inside/outside  |  target");
		foreach (EnemyAI e in list)
		{
			string distance = me != null ? Vector3.Distance(me.transform.position, e.transform.position).ToString("0") + "m" : "-";
			l.Add((e.isEnemyDead ? "[dead] " : "") + e.ShownName() + " (" + e.GetType().Name + ")  |  state " + e.currentBehaviourStateIndex
				+ (EnemyHunt.IsChasing(e) ? " (chasing)" : "")
				+ "  |  owner " + (e.IsOwner ? "you" : e.OwnerClientId.ToString())
				+ "  |  " + distance + "  |  " + (e.IsOutsideNow() ? "outside" : "inside")
				+ "  |  " + (e.targetPlayer != null ? e.targetPlayer.playerUsername : "-")
				+ (EnemyHunt.Find(e) != null ? "  |  HUNTING" : "")
				+ ((object)EnemyControl.ControlledEnemy == e ? "  |  CONTROLLED" : ""));
		}
		return l;
	}

	private void DrawErrors()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Clear"))
		{
			DebugLog.Clear();
		}
		DebugLog.MenuOnly = GUILayout.Toggle(DebugLog.MenuOnly, " Only the menu's own");
		GUILayout.EndHorizontal();
		GUILayout.Label("Errors and warnings since the menu loaded, newest at the bottom. Repeats are counted (x5).");
		if (logSnapshot.Count == 0)
		{
			GUILayout.Label("Nothing logged.");
		}
		foreach (DebugLog.Entry entry in logSnapshot)
		{
			string color = entry.Type == LogType.Warning ? "#e6c35c" : (entry.Type == LogType.Log ? "#cccccc" : "#ff6b6b");
			string head = entry.Time + "  " + (entry.FromMenu ? "[menu] " : "[game] ") + entry.Message + (entry.Count > 1 ? "  (x" + entry.Count + ")" : "");
			GUILayout.Label("<color=" + color + ">" + head + "</color>" + (string.IsNullOrEmpty(entry.Where) ? "" : "\n      " + entry.Where));
		}
		GUILayout.Space(8f);
		GUILayout.Label("Menu debug message:");
		if (GUILayout.Button("Clear Debug Message"))
		{
			Settings.debugMessage = "";
		}
		GUILayout.TextArea(Settings.debugMessage ?? "", GUILayout.Height(100f));
	}

	private void MenuContent()
	{
		GUILayout.Label("Developer tools from the original menu (results go to the log or the Errors section).");
		if (LethalMenu.localPlayer == null || StartOfRound.Instance == null)
		{
			GUILayout.Label("Join a lobby to use these.");
			return;
		}
		UI.Button("LookAt Closest Item", () =>
		{
			GrabbableObject grabbableObject = (from i in LethalMenu.items
				where !i.isInShipRoom
				orderby Vector3.Distance(i.transform.position, LethalMenu.localPlayer.transform.position)
				select i).FirstOrDefault();
			if (!(grabbableObject == null))
			{
				LethalMenu.localPlayer.transform.LookAt(grabbableObject.transform.position);
			}
		});
		UI.Button("LookAt Closest Player", () =>
		{
			PlayerControllerB playerControllerB = (from p in LethalMenu.players
				where p != LethalMenu.localPlayer
				orderby Vector3.Distance(p.transform.position, LethalMenu.localPlayer.transform.position)
				select p).FirstOrDefault();
			if (!(playerControllerB == null))
			{
				LethalMenu.localPlayer.transform.LookAt(playerControllerB.transform.position);
			}
		});
		GUILayout.Label("Debug Menu");
		GUILayout.BeginHorizontal();
		GUILayout.Label("Leaderboard");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			Leaderboard();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Debug NetworkObjectReferences");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			NetworkManager.Singleton.SpawnManager.SpawnedObjects.ToList().ForEach((KeyValuePair<ulong, NetworkObject> s) =>
			{
				Debug.Log($"Name: {s.Value.gameObject.name} ID: {s.Key}");
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Debug GetSpawnableMapObjects");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			GameUtil.GetSpawnableMapObjects().ToList().ForEach((SpawnableMapObject o) =>
			{
				Debug.Log(o.prefabToSpawn.name);
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Goto Not Spawned");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			LethalMenu.localPlayer.TeleportPlayer(StartOfRound.Instance.notSpawnedPosition.position);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Debug all layers");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			List<int> layers = new List<int>();
			(from o in UnityEngine.Object.FindObjectsOfType<GameObject>()
				where o != null
				select o).ToList().ForEach((GameObject o) =>
			{
				if (!layers.Contains(o.layer))
				{
					layers.Add(o.layer);
				}
				(from c in o.GetComponents<Component>()
					where c != null
					select c).ToList().ForEach((Component c) =>
				{
					if (!layers.Contains(c.gameObject.layer))
					{
						layers.Add(c.gameObject.layer);
					}
				});
			});
			layers.ForEach((int i) =>
			{
				Debug.Log($"Layer: {LayerMask.LayerToName(i)} ID: {i}");
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Raycast Colliders");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			Settings.debugMessage = "";
			RaycastHit[] array = CameraManager.ActiveCamera.transform.SphereCastForward();
			foreach (RaycastHit raycastHit in array)
			{
				Collider collider = raycastHit.collider;
				Settings.debugMessage += $"Hit: {collider.name} => {collider.gameObject.name} => Layer {LayerMask.LayerToName(collider.gameObject.layer)} {collider.gameObject.layer}\n";
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Garage");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			LethalMenu.animatedTriggers.Where((AnimatedObjectTrigger t) => t.name == "Cube" && t.transform.parent.name == "Cutscenes").ToList().ForEach((AnimatedObjectTrigger t) =>
			{
				t.triggerAnimator?.SetTrigger("doorFall");
			});
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Debug all components of held item");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Execute"))
		{
			LethalMenu.localPlayer?.currentlyHeldObjectServer?.GetComponents<Component>().Where((Component c) => c != null).ToList()
				.ForEach((Component c) =>
				{
					Debug.Log(c.GetType().FullName ?? "");
				});
		}
		GUILayout.EndHorizontal();
	}

	private async void Leaderboard()
	{
		LeaderboardUpdate? nullable = await (await SteamUserStats.FindOrCreateLeaderboardAsync($"challenge{GameNetworkManager.Instance.GetWeekNumber()}", LeaderboardSort.Descending, LeaderboardDisplay.Numeric)).Value.ReplaceScore(int.MaxValue);
		Settings.debugMessage = nullable.Value.OldGlobalRank + " => " + nullable.Value.NewGlobalRank;
	}
}
