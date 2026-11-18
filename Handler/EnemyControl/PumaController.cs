using System.Collections.Generic;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.AI;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal enum PumaState
{
	ROAMING,
	STALKING,
	ATTACKING
}

/// <summary>
/// Feiopar. Walks with the stalking walk, runs when you sprint. Left click attacks the nearest player (its attack
/// lasts a few seconds, like in the game); hold right click to scratch whoever it touches.
/// </summary>
internal class PumaController : IEnemyController<PumaAI>, IController
{
	/// <summary>How close (from its claws) a player must be to get scratched.</summary>
	private const float ScratchReach = 2.6f;

	private const float ScratchGap = 0.5f;

	/// <summary>When each player (by client id) can be scratched again.</summary>
	private readonly Dictionary<ulong, float> nextScratch = new Dictionary<ulong, float>();

	public void OnTakeControl(PumaAI enemy)
	{
		nextScratch.Clear();
		// Taken while up a tree or frozen mid-stalk, it stayed frozen for good: nothing on the driven path ends
		// those. Put it down on the ground before the camera rig is placed on it.
		PumaAIPatches.PutOnGround(enemy);
		// Its "roaming" mode is a sprint to the nearest tree (and hands the Feiopar back to the host);
		// driven, it stays in its stalking mode, which has the walk animation.
		if (!enemy.IsBehaviourState(PumaState.STALKING))
		{
			enemy.SetBehaviourState(PumaState.STALKING);
		}
	}

	public void Update(PumaAI enemy, bool isAIControlled)
	{
		if (!isAIControlled)
		{
			// Keep the puma on the ground while it is being driven.
			enemy.clingingToTree = false;
			if (enemy.IsBehaviourState(PumaState.ATTACKING))
			{
				ScratchNearby(enemy);
			}
		}
	}

	public void UsePrimarySkill(PumaAI enemy)
	{
		PlayerControllerB? player = Control.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 25f);
		if (player == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No player in pounce range");
			return;
		}
		enemy.targetPlayer = player;
		enemy.SetBehaviourState(PumaState.ATTACKING);
	}

	public void OnSecondarySkillHold(PumaAI enemy)
	{
		ScratchNearby(enemy);
	}

	/// <summary>
	/// The game hurts a player only on that player's own machine, and only right after its scratch animation
	/// played there. That animation never played on the other players' machines (there the Feiopar was after your
	/// own body), so nobody else got hurt. The scratches are dealt from here instead.
	/// </summary>
	private void ScratchNearby(PumaAI enemy)
	{
		PlayerControllerB you = LethalMenu.localPlayer;
		if (StartOfRound.Instance == null || you == null || enemy.isEnemyDead)
		{
			return;
		}
		Vector3 claws = enemy.pushPoint != null ? enemy.pushPoint.position : enemy.transform.position + Vector3.up;
		foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
		{
			if (player == null || !player.isPlayerControlled || player.isPlayerDead)
			{
				continue;
			}
			bool isYou = player == you;
			if (isYou && !Control.TargetingYourself)
			{
				continue;
			}
			if (Vector3.Distance(claws, player.transform.position + Vector3.up * 0.9f) > ScratchReach)
			{
				continue;
			}
			if (nextScratch.TryGetValue(player.playerClientId, out float readyAt) && Time.time < readyAt)
			{
				continue;
			}
			nextScratch[player.playerClientId] = Time.time + ScratchGap;
			Vector3 push = Vector3.Normalize(player.playerEye.transform.position - claws) * enemy.scratchPushForce;
			if (isYou)
			{
				// Your own body: the same as the game does on the scratched player's machine.
				player.externalForceAutoFade += push;
				player.DamagePlayer(7, true, true, CauseOfDeath.Scratching, 10, false, push * 1.25f);
				HUDManager.Instance?.ShakeCamera(ScreenShakeType.Small);
			}
			else
			{
				player.DamagePlayerFromOtherClientServerRpc(7, (player.transform.position - enemy.transform.position).normalized, (int)you.playerClientId);
			}
			// Blood and face scratches (the call reaches everyone but us).
			player.AddBloodToBody();
			if (player.scratchesOnFace != null)
			{
				player.scratchesOnFace.SetActive(true);
			}
			enemy.PumaDamagePlayerRpc((int)player.playerClientId);
		}
	}

	public bool IsAbleToMove(PumaAI enemy)
	{
		// Just dropped from a tree: a moment still, so your position follows it down instead of pulling it back up.
		return !enemy.inSpecialAnimation && Time.time >= PumaAIPatches.DroppedFromTreeUntil;
	}

	public string GetPrimarySkillName(PumaAI _)
	{
		return "Attack nearest player";
	}

	public string GetSecondarySkillName(PumaAI _)
	{
		return "(HOLD) Scratch on contact";
	}

	public bool CanUseEntranceDoors(PumaAI _)
	{
		return false;
	}

	public float SprintMultiplier(PumaAI _)
	{
		return 3.5f;
	}
}

/// <summary>
/// The Feiopar's animations come from its AI mode: in "roaming" it plays its run (it sprints to a tree), in
/// "stalking" a walk whose speed follows how fast its AI wants to go. So it ran in place standing still and
/// walked while you sprinted. Driven, its animation follows what you actually do.
/// </summary>
[HarmonyPatch]
internal static class PumaAIPatches
{
	private static readonly MethodInfo CalculateAnimationDirection = AccessTools.Method(typeof(PumaAI), "CalculateAnimationDirection");

	private static readonly AccessTools.FieldRef<PumaAI, bool> StalkingFrozenAnim = AccessTools.FieldRefAccess<PumaAI, bool>("stalkingFrozenAnim");

	private static readonly AccessTools.FieldRef<PumaAI, float> SyncVelocityInterval = AccessTools.FieldRefAccess<PumaAI, float>("syncVelocityInterval");

	private static readonly AccessTools.FieldRef<PumaAI, float> VelX = AccessTools.FieldRefAccess<PumaAI, float>("velX");

	private static readonly AccessTools.FieldRef<PumaAI, float> VelZ = AccessTools.FieldRefAccess<PumaAI, float>("velZ");

	/// <summary>Until when the Feiopar just dropped from a tree stays still (read by <see cref="PumaController"/>).</summary>
	internal static float DroppedFromTreeUntil;

	/// <summary>
	/// Ends tree clinging and the stalk freeze. Both stop it from moving, and the stalking code that would end them
	/// never runs while you drive. Up a tree it is also off the ground with its pathfinding off: drop it down.
	/// </summary>
	internal static void PutOnGround(PumaAI enemy)
	{
		enemy.inSpecialAnimation = false;
		enemy.clingingToTree = false;
		enemy.clingingToTreeAnim = false;
		if (!enemy.IsOwner || enemy.agent == null || enemy.agent.enabled)
		{
			return;
		}
		Vector3 position = enemy.transform.position;
		Vector3 ground = position;
		if (Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 80f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
		{
			ground = hit.point;
		}
		if (NavMesh.SamplePosition(ground, out NavMeshHit navHit, 5f, NavMesh.AllAreas) || NavMesh.SamplePosition(position, out navHit, 40f, NavMesh.AllAreas))
		{
			ground = navHit.position;
		}
		enemy.transform.position = ground;
		enemy.agent.enabled = true;
		enemy.agent.Warp(ground);
		DroppedFromTreeUntil = Time.time + 0.15f;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PumaAI), nameof(PumaAI.Update))]
	public static void UpdatePrefix(PumaAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead)
		{
			return;
		}
		// An attack ends by going back to roaming; come back to stalking instead.
		if (__instance.currentBehaviourStateIndex == (int)PumaState.ROAMING)
		{
			__instance.SwitchToBehaviourState((int)PumaState.STALKING);
		}
		// Without a target its stalking code stops before its own animation step (which the postfix does instead)
		// and doesn't hand the Feiopar over to the player it was stalking.
		if (__instance.currentBehaviourStateIndex == (int)PumaState.STALKING)
		{
			__instance.targetPlayer = null;
		}
		StalkingFrozenAnim(__instance) = false;
		__instance.stalkingFrozen = false;
		// Also when you take back over from F9 after its AI went up a tree or froze.
		if (__instance.currentBehaviourStateIndex != (int)PumaState.ATTACKING && (__instance.inSpecialAnimation || __instance.clingingToTreeAnim || (__instance.IsOwner && __instance.agent != null && !__instance.agent.enabled)))
		{
			PutOnGround(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PumaAI), nameof(PumaAI.Update))]
	public static void UpdatePostfix(PumaAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead || __instance.creatureAnimator == null)
		{
			return;
		}
		Animator animator = __instance.creatureAnimator;
		bool moving = Control.YouAreMoving;
		animator.SetBool("Climbing", false);
		animator.SetBool("TreeMode", false);
		animator.SetBool("Startled", false);
		animator.SetBool("Landed", true);
		if (__instance.currentBehaviourStateIndex == (int)PumaState.ATTACKING)
		{
			return; // its attack run and scratching are animated by the game
		}
		animator.SetBool("Running", Control.YouAreSprinting);
		// The walk's speed follows agent.speed (it isn't what moves it while you drive); 0 stands still.
		if (__instance.agent != null)
		{
			__instance.agent.speed = moving ? 6f : 0f;
		}
		if (!Control.YouAreSprinting || !__instance.IsOwner)
		{
			CalculateAnimationDirection?.Invoke(__instance, new object[1] { 1f });
			return;
		}
		// The run is only shown here; other players saw the slow stalking walk while it raced past them. While you
		// sprint, send them its fastest walk instead (the game's own sync would send the normal speed).
		float untilSync = SyncVelocityInterval(__instance) - Time.deltaTime;
		SyncVelocityInterval(__instance) = 100f;
		CalculateAnimationDirection?.Invoke(__instance, new object[1] { 1f });
		if (untilSync <= 0f)
		{
			untilSync = 0.17f;
			__instance.SyncMovementVelocityRpc(VelX(__instance), VelZ(__instance), 1.3f);
		}
		SyncVelocityInterval(__instance) = untilSync;
	}
}
