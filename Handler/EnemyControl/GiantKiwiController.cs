using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal enum GiantKiwiState
{
	IDLE,
	PATROLLING,
	ATTACKING
}

/// <summary>
/// Big Bird. Left click screeches. Hold right click to peck: with a player within 40 m it goes after them (and
/// pecks them once close, with its normal damage); with nobody around it pecks the ground where it stands.
/// </summary>
internal class GiantKiwiController : IEnemyController<GiantKiwiAI>, IController
{
	/// <summary>True while right click is held (read by <see cref="GiantKiwiAIPatches"/>).</summary>
	internal static bool PeckHeld;

	private bool attacking;

	public void OnTakeControl(GiantKiwiAI enemy)
	{
		PeckHeld = false;
		attacking = false;
		// Out of its idle routine (dancing, pecking a tree, sleeping at the nest) so it walks when you walk.
		if (enemy.IsBehaviourState(GiantKiwiState.IDLE))
		{
			enemy.SetBehaviourState(GiantKiwiState.PATROLLING);
		}
	}

	public void UsePrimarySkill(GiantKiwiAI enemy)
	{
		enemy.Screech(enraged: true, sync: true);
	}

	public void OnSecondarySkillHold(GiantKiwiAI enemy)
	{
		PeckHeld = true;
		if (attacking)
		{
			return;
		}
		PlayerControllerB? player = Control.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 40f);
		if (player == null)
		{
			return; // nobody to go after: it just pecks where it stands (see the patch below)
		}
		attacking = true;
		enemy.targetPlayer = player;
		enemy.Reflect().SetValue("watchingThreat", player);
		enemy.SyncWatchingThreatServerRpc(player.NetworkObject, (int)LethalMenu.localPlayer.playerClientId);
		enemy.SetBehaviourState(GiantKiwiState.ATTACKING);
	}

	public void ReleaseSecondarySkill(GiantKiwiAI enemy)
	{
		PeckHeld = false;
		StopAttacking(enemy);
	}

	public void OnReleaseControl(GiantKiwiAI enemy)
	{
		PeckHeld = false;
		StopAttacking(enemy);
	}

	private void StopAttacking(GiantKiwiAI enemy)
	{
		if (attacking)
		{
			attacking = false;
			enemy.SetBehaviourState(GiantKiwiState.PATROLLING);
		}
	}

	public bool IsAbleToMove(GiantKiwiAI enemy)
	{
		return !enemy.inKillAnimation;
	}

	public string GetPrimarySkillName(GiantKiwiAI _)
	{
		return "Screech";
	}

	public string GetSecondarySkillName(GiantKiwiAI _)
	{
		return "(HOLD) Peck";
	}

	public bool CanUseEntranceDoors(GiantKiwiAI _)
	{
		return false;
	}

	public float InteractRange(GiantKiwiAI _)
	{
		return 0f;
	}
}

/// <summary>Keeps a driven Big Bird out of its own routines, and pecks while right click is held.</summary>
[HarmonyPatch]
internal static class GiantKiwiAIPatches
{
	private static readonly AccessTools.FieldRef<GiantKiwiAI, int> IdleBehaviour = AccessTools.FieldRefAccess<GiantKiwiAI, int>("idleBehaviour");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, float> BehaviourTimer = AccessTools.FieldRefAccess<GiantKiwiAI, float>("behaviourTimer");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, bool> IsPeckingTree = AccessTools.FieldRefAccess<GiantKiwiAI, bool>("isPeckingTree");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, bool> Attacking = AccessTools.FieldRefAccess<GiantKiwiAI, bool>("attacking");

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GiantKiwiAI), nameof(GiantKiwiAI.Update))]
	public static void Update(GiantKiwiAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead || __instance.inKillAnimation || __instance.creatureAnimator == null)
		{
			return;
		}
		// Every 18 s the bird switched by itself between dancing, pecking a tree and sitting down to sleep, even
		// while you walked it around. Stay on "awake".
		// (The same timer is the short wind-up before an attack in its attack mode, so only touch it while idle.)
		if (__instance.currentBehaviourStateIndex == 0)
		{
			BehaviourTimer(__instance) = Mathf.Max(BehaviourTimer(__instance), 5f);
		}
		if (IdleBehaviour(__instance) != 0 && __instance.IsServer)
		{
			IdleBehaviour(__instance) = 0;
			__instance.ChangeIdleBehaviorClientRpc(0);
		}
		IsPeckingTree(__instance) = false;
		__instance.creatureAnimator.SetBool("Asleep", false);
		__instance.creatureAnimator.SetBool("peckTree", false);
		__instance.creatureAnimator.SetBool("Dancing", false);
		// The game only pecks with a target within 10 m. Holding right click pecks anyway.
		if (GiantKiwiController.PeckHeld && __instance.stunNormalizedTimer <= 0f)
		{
			Attacking(__instance) = true;
			__instance.creatureAnimator.SetBool("Attacking", true);
			__instance.creatureAnimator.SetFloat("attackSpeed", 1f);
		}
	}
}
