using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using UnityEngine;

namespace LethalMenu.Cheats;

/// <summary>
/// Hunt: a one-time nudge. The monster walks toward one player, even from across the map, at its own pace and
/// without being angered. Every time its AI thinks it is pointed back at the player (EnemyHuntPatches), so its
/// wandering can't pull it away. The hunt is over for good the moment the monster sees the player, reaches them,
/// or starts chasing anyone: from then on only its normal AI decides what it does (chase, lose them, wander off).
/// Only works inside-to-inside or outside-to-outside: it never moves a monster between the facility and outside.
/// </summary>
internal class EnemyHunt : Cheat
{
	internal class Hunt
	{
		public EnemyAI Enemy;

		public PlayerControllerB Target;

		public float StartTime;

		public float NextOwnerRequest;

		/// <summary>Persistent hunt: never ends by itself; keeps going at the player until stopped or they die.</summary>
		public bool Persistent;

		/// <summary>Aggro hunt (Eyeless Dogs): walks until close, then is enraged and sprints at the player.</summary>
		public bool Aggro;

		public float NextEnrage;

		/// <summary>Kill hunt: a persistent hunt that, once close, makes the monster angry at the player so it
		/// attacks with its normal damage. Ends only when the player is dead (or it is stopped).</summary>
		public bool Kill;
	}

	/// <summary>How close a kill hunt gets before the monster is made angry at the player.</summary>
	internal const float KillAngerRange = 15f;

	/// <summary>Monsters that can't hurt anyone, so "Kill player" isn't offered for them.</summary>
	internal static readonly string[] Harmless = new string[] { "DocileLocustBeesAI", "DoublewingAI", "FlowerSnakeEnemy", "DressGirlAI" };

	/// <summary>How close an aggro-hunting dog walks before it is enraged and sprints.</summary>
	internal const float AggroRange = 12f;

	private const float GiveUpAfter = 180f;

	/// <summary>This close counts as having reached the player, seen or not.</summary>
	private const float ReachedRange = 4f;

	/// <summary>How far away, and how far off to the side (degrees), the monster can spot the player.</summary>
	private const int SightRange = 30;

	private const float SightHalfAngle = 70f;

	/// <summary>
	/// Uses the game's own "can this monster see that spot" check (from its eyes, blocked by walls), so it stops
	/// being steered at the same moment its normal AI could notice the player by itself.
	/// </summary>
	internal static bool Sees(EnemyAI enemy, PlayerControllerB player)
	{
		// Eyeless Dogs are blind: they only stop for reaching the player or their own (hearing) chase.
		if (enemy is MouthDogAI)
		{
			return false;
		}
		Vector3 head = player.gameplayCamera != null ? player.gameplayCamera.transform.position : player.transform.position + Vector3.up * 1.5f;
		if (!enemy.CheckLineOfSightForPosition(head, SightHalfAngle, SightRange, ReachedRange))
		{
			return false;
		}
		// A monster's eyes can poke through a thin wall (the ship's hull) and "see" someone it would still have
		// to walk all the way around to reach. Only count it when the walking route is about as short as the
		// straight line, i.e. they really are in the open together.
		float direct = Vector3.Distance(enemy.transform.position, player.transform.position);
		return WalkDistance(enemy) <= direct * 1.4f + 3f;
	}

	/// <summary>Length of the route the monster is currently walking (a huge number when there is none yet).</summary>
	internal static float WalkDistance(EnemyAI enemy)
	{
		UnityEngine.AI.NavMeshAgent agent = enemy.agent;
		if (agent == null || !agent.enabled || !agent.isOnNavMesh || agent.pathPending || !agent.hasPath)
		{
			return float.MaxValue;
		}
		Vector3[] corners = agent.path.corners;
		float length = 0f;
		Vector3 previous = enemy.transform.position;
		for (int i = 0; i < corners.Length; i++)
		{
			length += Vector3.Distance(previous, corners[i]);
			previous = corners[i];
		}
		return length;
	}

	/// <summary>
	/// Gives the monster back to its own AI: drop the path toward the player and make the game re-apply wherever
	/// the AI itself wanted to go (it only does that when it thinks the destination changed).
	/// </summary>
	private static void HandBack(EnemyAI enemy)
	{
		if (enemy == null || enemy.isEnemyDead || !enemy.IsOwner)
		{
			return;
		}
		try
		{
			if (enemy.agent != null && enemy.agent.enabled && enemy.agent.isOnNavMesh)
			{
				enemy.agent.ResetPath();
			}
			HarmonyLib.Traverse.Create(enemy).Field("prevDestination").SetValue(Vector3.zero);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Hunt hand-back: " + ex.Message);
		}
	}

	/// <summary>
	/// The behaviour states in which each monster is angry and chasing/attacking, checked against the game's own AI
	/// code (where each AI switches into that state). Not chase states, on purpose: Bracken 1 (stalking/evading),
	/// Maneater 2 (sneaking after you), Masked 2 (heading to the ship), Old Bird 2 (flying to a new spot),
	/// Nutcracker 1 (inspecting), Jester 1 (cranking), Eyeless Dog 1 (suspicious).
	/// Monsters not listed count as chasing when they move straight at a player (the Blob uses its anger timer).
	/// </summary>
	private static readonly Dictionary<string, int[]> ChaseStates = new Dictionary<string, int[]>
	{
		{ "BaboonBirdAI", new int[] { 2 } },
		{ "BushWolfEnemy", new int[] { 1, 2 } },
		{ "CadaverBloomAI", new int[] { 1 } },
		{ "CaveDwellerAI", new int[] { 3 } },
		{ "ButlerEnemyAI", new int[] { 2 } },
		{ "CentipedeAI", new int[] { 2, 3 } },
		{ "CrawlerAI", new int[] { 1 } },
		{ "DressGirlAI", new int[] { 1 } },
		{ "FlowerSnakeEnemy", new int[] { 1 } },
		{ "FlowermanAI", new int[] { 2 } },
		{ "ForestGiantAI", new int[] { 1 } },
		{ "GiantKiwiAI", new int[] { 2 } },
		{ "HoarderBugAI", new int[] { 2 } },
		{ "JesterAI", new int[] { 2 } },
		{ "LassoManAI", new int[] { 1 } },
		{ "MaskedPlayerEnemy", new int[] { 1 } },
		{ "MouthDogAI", new int[] { 2, 3 } },
		{ "NutcrackerEnemyAI", new int[] { 2 } },
		{ "PufferAI", new int[] { 2 } },
		{ "PumaAI", new int[] { 2 } },
		{ "RadMechAI", new int[] { 1 } },
		{ "RedLocustBees", new int[] { 2 } },
		{ "SandSpiderAI", new int[] { 2 } },
		{ "SandWormAI", new int[] { 1 } },
		{ "SpringManAI", new int[] { 1 } }
	};

	internal static bool IsChasing(EnemyAI enemy)
	{
		if (enemy is BlobAI)
		{
			return HarmonyLib.Traverse.Create(enemy).Field("angeredTimer").GetValue<float>() > 0f;
		}
		if (ChaseStates.TryGetValue(enemy.GetType().Name, out int[] states))
		{
			return Array.IndexOf(states, enemy.currentBehaviourStateIndex) >= 0;
		}
		return enemy.movingTowardsTargetPlayer && enemy.targetPlayer != null;
	}

	internal static readonly List<Hunt> Hunts = new List<Hunt>();

	internal static Hunt Find(EnemyAI enemy)
	{
		for (int i = 0; i < Hunts.Count; i++)
		{
			if ((object)Hunts[i].Enemy == enemy)
			{
				return Hunts[i];
			}
		}
		return null;
	}

	internal static bool SameArea(EnemyAI enemy, PlayerControllerB player)
	{
		return enemy.isOutside == !player.isInsideFactory;
	}

	internal static string EnemyName(EnemyAI enemy)
	{
		return enemy.ShownName();
	}

	/// <summary>Why this hunt can't start, or null if it can.</summary>
	internal static string WhyNot(EnemyAI enemy, PlayerControllerB player, bool persistent = false, bool aggro = false, bool kill = false)
	{
		if (kill && enemy != null && Array.IndexOf(Harmless, enemy.GetType().Name) >= 0)
		{
			return EnemyName(enemy) + " can't hurt anyone";
		}
		if (aggro && enemy != null && !(enemy is MouthDogAI))
		{
			return "Aggro Hunt only works on Eyeless Dogs";
		}
		if (enemy == null || enemy.isEnemyDead)
		{
			return "That monster is dead or gone";
		}
		if (player == null || !player.isPlayerControlled || player.isPlayerDead)
		{
			return "Pick a living player";
		}
		if (enemy.agent == null)
		{
			return EnemyName(enemy) + " can't walk anywhere";
		}
		if (enemy.daytimeEnemyLeaving)
		{
			return EnemyName(enemy) + " is leaving for the day";
		}
		if (persistent || aggro || kill)
		{
			return null; // it waits if they are in the other area, and takes over an existing chase
		}
		if (IsChasing(enemy))
		{
			return EnemyName(enemy) + " is already chasing someone";
		}
		if (!SameArea(enemy, player))
		{
			return EnemyName(enemy) + " is " + (enemy.isOutside ? "outside" : "inside") + " and " + player.playerUsername + " is " + (player.isInsideFactory ? "inside" : "outside") + "; it can't get there";
		}
		return null;
	}

	/// <summary>Starts (or retargets) a hunt. Returns a message for the player; throws if it can't start.</summary>
	internal static string Begin(EnemyAI enemy, PlayerControllerB player, bool persistent = false, bool aggro = false, bool kill = false)
	{
		string problem = WhyNot(enemy, player, persistent, aggro, kill);
		if (problem != null)
		{
			throw new InvalidOperationException(problem);
		}
		Hunt hunt = Find(enemy);
		if (hunt == null)
		{
			hunt = new Hunt { Enemy = enemy };
			Hunts.Add(hunt);
		}
		hunt.Target = player;
		hunt.Persistent = persistent || aggro || kill;
		hunt.Aggro = aggro;
		hunt.Kill = kill;
		hunt.NextEnrage = 0f;
		hunt.StartTime = Time.time;
		hunt.NextOwnerRequest = 0f;
		if (kill)
		{
			return EnemyName(enemy) + " is going after " + player.playerUsername + " and won't stop until they're dead";
		}
		if (aggro)
		{
			return EnemyName(enemy) + " is stalking " + player.playerUsername + " and will charge when close";
		}
		return EnemyName(enemy) + (persistent ? " will keep hunting " + player.playerUsername + " until stopped" : " is walking to " + player.playerUsername);
	}

	/// <summary>
	/// Starts a one-shot hunt on the player with every living monster on the map. Monsters that can't (other
	/// area, already chasing, the one you control) are skipped. Returns a message saying how many went.
	/// </summary>
	internal static string BeginAll(PlayerControllerB player)
	{
		if (player == null || !player.isPlayerControlled || player.isPlayerDead)
		{
			throw new InvalidOperationException("Pick a living player");
		}
		int started = 0;
		int skipped = 0;
		foreach (EnemyAI enemy in LethalMenu.enemies.ToList())
		{
			if (enemy == null || enemy.isEnemyDead || (object)EnemyControl.ControlledEnemy == enemy)
			{
				continue;
			}
			if (WhyNot(enemy, player) != null)
			{
				skipped++;
				continue;
			}
			Begin(enemy, player);
			started++;
		}
		if (started == 0)
		{
			return skipped == 0
				? "There are no living monsters"
				: "None of the " + skipped + " monster" + (skipped == 1 ? "" : "s") + " can go to " + player.playerUsername + " (other area or already chasing)";
		}
		return started + " monster" + (started == 1 ? " is" : "s are") + " walking to " + player.playerUsername + (skipped > 0 ? " (" + skipped + " couldn't)" : "");
	}

	internal static bool Stop(EnemyAI enemy)
	{
		Hunt hunt = Find(enemy);
		if (hunt == null)
		{
			return false;
		}
		Hunts.Remove(hunt);
		HandBack(enemy);
		return true;
	}

	internal static int StopAll()
	{
		int count = Hunts.Count;
		foreach (Hunt hunt in Hunts)
		{
			HandBack(hunt.Enemy);
		}
		Hunts.Clear();
		return count;
	}

	public override void Update()
	{
		if (Hunts.Count == 0)
		{
			return;
		}
		PlayerControllerB me = LethalMenu.localPlayer;
		for (int i = Hunts.Count - 1; i >= 0; i--)
		{
			Hunt hunt = Hunts[i];
			string end = EndReason(hunt);
			if (end != null)
			{
				Hunts.RemoveAt(i);
				HandBack(hunt.Enemy);
				if (end.Length > 0 && HUDManager.Instance != null)
				{
					HUDManager.Instance.DisplayTip("Lethal Menu", end);
				}
				continue;
			}
			EnemyAI enemy = hunt.Enemy;
			// Only the owner's game runs a monster's AI, so we need to own it to steer it.
			if (!enemy.IsOwner && me != null && Time.time >= hunt.NextOwnerRequest)
			{
				hunt.NextOwnerRequest = Time.time + 1f;
				enemy.ChangeEnemyOwnerServerRpc(me.actualClientId);
			}
		}
	}

	/// <summary>Null = keep going; "" = stop quietly; otherwise stop and show this.</summary>
	private static string EndReason(Hunt hunt)
	{
		EnemyAI enemy = hunt.Enemy;
		PlayerControllerB target = hunt.Target;
		if (enemy == null || enemy.isEnemyDead)
		{
			return "";
		}
		string name = EnemyName(enemy);
		if (enemy.daytimeEnemyLeaving)
		{
			// Daytime monsters (Manticoils, bees...) all leave at a set time of day: they fly off and vanish.
			return name + " is leaving for the day; its hunt is over";
		}
		if (target == null || !target.isPlayerControlled && !target.isPlayerDead)
		{
			return name + " stopped hunting: the player left";
		}
		if (target.isPlayerDead)
		{
			if (hunt.Kill)
			{
				return target.playerUsername + " is dead; " + name + " stopped hunting";
			}
			return hunt.Persistent ? name + "'s hunt is over: " + target.playerUsername + " is dead" : "";
		}
		if (hunt.Persistent)
		{
			return null; // never ends by itself: no sight, reach, chase or time limit
		}
		if (!SameArea(enemy, target))
		{
			return name + " stopped hunting " + target.playerUsername + ": they went " + (target.isInsideFactory ? "inside" : "outside");
		}
		// It got angry (at them, or at someone it ran into on the way): its own behaviour takes over from here.
		if (IsChasing(enemy))
		{
			return name + " is chasing " + (enemy.targetPlayer != null ? enemy.targetPlayer.playerUsername : "someone") + " on its own";
		}
		// One-shot: as soon as it can see them (or is right on top of them) the nudge is over. What happens next
		// is entirely its own AI: it may chase, ignore them, or lose them and wander off. It is never steered again.
		// "Reached" also needs a short walking route: 4m away through the ship's wall is not there yet.
		if (Vector3.Distance(enemy.transform.position, target.transform.position) <= ReachedRange && WalkDistance(enemy) <= ReachedRange * 2f)
		{
			return name + " reached " + target.playerUsername;
		}
		if (Sees(enemy, target))
		{
			return name + " spotted " + target.playerUsername + "; it's on its own now";
		}
		if (Time.time - hunt.StartTime > GiveUpAfter)
		{
			return name + " gave up on " + target.playerUsername + " after 3 minutes";
		}
		return null;
	}
}
