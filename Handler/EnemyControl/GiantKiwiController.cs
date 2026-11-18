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

	public void Update(GiantKiwiAI enemy, bool isAIControlled)
	{
		// The AI drives now (F9): a peck held from before must not carry over to when you take it back, where
		// the bird kept pecking and couldn't pick a new target.
		if (isAIControlled && (PeckHeld || attacking))
		{
			PeckHeld = false;
			StopAttacking(enemy);
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
		attacking = false;
		if (enemy == null)
		{
			return;
		}
		enemy.Reflect().SetValue("watchingThreat", null);
		enemy.Reflect().SetValue("attackingThreat", null);
		enemy.watchingThreatTransform = null;
		enemy.Reflect().SetValue("attacking", false);
		enemy.creatureAnimator?.SetBool("Attacking", false);
		if (enemy.IsBehaviourState(GiantKiwiState.ATTACKING))
		{
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

	private static readonly AccessTools.FieldRef<GiantKiwiAI, AudioSource> BirdNestAmbience = AccessTools.FieldRefAccess<GiantKiwiAI, AudioSource>("birdNestAmbience");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, float> PingAttentionTimer = AccessTools.FieldRefAccess<GiantKiwiAI, float>("pingAttentionTimer");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, IVisibleThreat> WatchingThreat = AccessTools.FieldRefAccess<GiantKiwiAI, IVisibleThreat>("watchingThreat");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, IVisibleThreat> AttackingThreat = AccessTools.FieldRefAccess<GiantKiwiAI, IVisibleThreat>("attackingThreat");

	private static readonly AccessTools.FieldRef<GiantKiwiAI, float> WalkAnimSpeed = AccessTools.FieldRefAccess<GiantKiwiAI, float>("walkAnimSpeed");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GiantKiwiAI), nameof(GiantKiwiAI.Update))]
	public static void UpdatePrefix(GiantKiwiAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead)
		{
			return;
		}
		if (__instance.birdNest == null || BirdNestAmbience(__instance) == null)
		{
			Transform fallback = __instance.transform.Find("FallbackBirdNest");
			if (fallback == null)
			{
				GameObject go = new GameObject("FallbackBirdNest");
				go.transform.SetParent(__instance.transform, worldPositionStays: false);
				fallback = go.transform;
			}
			__instance.birdNest = fallback.gameObject;
			BirdNestAmbience(__instance) = fallback.GetComponent<AudioSource>() ?? fallback.gameObject.AddComponent<AudioSource>();
		}
		PingAttentionTimer(__instance) = -1f;
		if (!GiantKiwiController.PeckHeld)
		{
			WatchingThreat(__instance) = null;
			AttackingThreat(__instance) = null;
			__instance.watchingThreatTransform = null;
			if (__instance.currentBehaviourStateIndex == 2)
			{
				__instance.SetBehaviourState(GiantKiwiState.PATROLLING);
			}
		}
		else if (__instance.currentBehaviourStateIndex == 2)
		{
			BehaviourTimer(__instance) = 0f;
		}
	}

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
		// The game only pecks with a target within 10 m. Holding right click pecks anyway; releasing stops immediately.
		if (GiantKiwiController.PeckHeld && __instance.stunNormalizedTimer <= 0f)
		{
			Attacking(__instance) = true;
			__instance.creatureAnimator.SetBool("Attacking", true);
			__instance.creatureAnimator.SetFloat("attackSpeed", 1f);
		}
		else
		{
			Attacking(__instance) = false;
			__instance.creatureAnimator.SetBool("Attacking", false);
		}
		bool sprinting = Control.YouAreMoving && Control.YouAreSprinting;
		float desiredWalkAnim = sprinting ? 1.8f : 1f;
		__instance.creatureAnimator.SetFloat("WalkSpeed", desiredWalkAnim);
		__instance.creatureAnimator.SetBool("Running", sprinting);
		if (Mathf.Abs(WalkAnimSpeed(__instance) - desiredWalkAnim) > 0.05f)
		{
			WalkAnimSpeed(__instance) = desiredWalkAnim;
			__instance.SyncWalkAnimSpeedServerRpc(desiredWalkAnim);
		}
	}
}
