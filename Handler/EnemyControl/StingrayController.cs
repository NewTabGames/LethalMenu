using System;
using System.Reflection;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal enum StingrayState
{
	ROAMING,
	CLOAKED
}

internal class StingrayController : IEnemyController<StingrayAI>, IController
{
	private static readonly MethodInfo? CreateSlimeLocal = AccessTools.Method(typeof(StingrayAI), "CreateSlime", new Type[1] { typeof(Vector3) });

	internal static bool WantsCloaked;

	private float slimeCooldown;

	/// <summary>Set when you uncloak; the lunge starts once it is really out of its cloak (after the host answers).</summary>
	private bool lungePending;

	public void OnTakeControl(StingrayAI enemy)
	{
		lungePending = false;
		WantsCloaked = enemy != null && enemy.IsBehaviourState(StingrayState.CLOAKED);
	}

	public void OnReleaseControl(StingrayAI enemy)
	{
		WantsCloaked = false;
	}

	public void Update(StingrayAI enemy, bool isAIControlled)
	{
		slimeCooldown -= Time.deltaTime;
		if (isAIControlled)
		{
			lungePending = false;
			WantsCloaked = false;
			return;
		}
		enemy.targetPlayer = null;
		if (enemy.stunNormalizedTimer > 0f)
		{
			WantsCloaked = false;
		}
		else if (WantsCloaked && !enemy.IsBehaviourState(StingrayState.CLOAKED))
		{
			enemy.SetBehaviourState(StingrayState.CLOAKED);
		}
		// Stop the AI cloaking on its own when it thinks it has reached a hiding spot.
		StingrayHidingSpot? hidingSpot = enemy.Reflect().GetValue<StingrayHidingSpot>("hidingSpot");
		if (hidingSpot != null)
		{
			hidingSpot.gotHidingSpot = false;
		}
		// The game's lunge is its pathfinding speed, which doesn't move it while you drive: it spat in place.
		if (lungePending && !enemy.IsBehaviourState(StingrayState.CLOAKED))
		{
			lungePending = false;
			Control.Dash(enemy.transform.forward, 8f, 0.6f);
		}
	}

	public void UsePrimarySkill(StingrayAI enemy)
	{
		// Leaving the cloaked state makes the game perform its lunge + spit.
		bool cloaked = enemy.IsBehaviourState(StingrayState.CLOAKED);
		lungePending = cloaked;
		WantsCloaked = !cloaked;
		enemy.SetBehaviourState(cloaked ? StingrayState.ROAMING : StingrayState.CLOAKED);
	}

	public void OnSecondarySkillHold(StingrayAI enemy)
	{
		if (slimeCooldown > 0f || enemy.IsBehaviourState(StingrayState.CLOAKED))
		{
			return;
		}
		slimeCooldown = 0.35f;
		Control.EnsureMapPropsContainer();
		Vector3 position = enemy.transform.position;
		// CreateSlime is overloaded (Vector3 / Vector3[]), so resolve the exact signature. The RPC only reaches other clients.
		CreateSlimeLocal?.Invoke(enemy, new object[1] { position });
		enemy.CreateSlimeRpc(position);
	}

	public bool IsAbleToMove(StingrayAI enemy)
	{
		return !enemy.IsBehaviourState(StingrayState.CLOAKED);
	}

	public string GetPrimarySkillName(StingrayAI enemy)
	{
		return enemy.IsBehaviourState(StingrayState.CLOAKED) ? "Lunge out of cloak" : "Cloak";
	}

	public string GetSecondarySkillName(StingrayAI _)
	{
		return "(HOLD) Leave slime trail";
	}

	public bool CanUseEntranceDoors(StingrayAI _)
	{
		return false;
	}
}

/// <summary>
/// On uncloaking, a Gunk Fish owned by a non-host hands itself to the host and skips the rest of its update that
/// frame. While you drive it that handoff is blocked, so for a non-host driver it skipped it every frame: no
/// walk animation, sounds, slime or spit lunge until you let go. Driven, it starts its uncloaked mode right here.
/// </summary>
[HarmonyPatch]
internal static class StingrayAIPatches
{
	private static readonly AccessTools.FieldRef<StingrayAI, int> PreviousBehaviourState = AccessTools.FieldRefAccess<StingrayAI, int>("previousBehaviourState");

	private static readonly AccessTools.FieldRef<StingrayAI, float> TimeSpentInState = AccessTools.FieldRefAccess<StingrayAI, float>("timeSpentInState");

	private static readonly AccessTools.FieldRef<StingrayAI, bool> HasSpit = AccessTools.FieldRefAccess<StingrayAI, bool>("hasSpit");

	private static readonly AccessTools.FieldRef<StingrayAI, float> WatchPlayerSpitTimer = AccessTools.FieldRefAccess<StingrayAI, float>("watchPlayerSpitTimer");

	private static readonly AccessTools.FieldRef<StingrayAI, float> TimeSinceHitting = AccessTools.FieldRefAccess<StingrayAI, float>("timeSinceHitting");

	private static readonly AccessTools.FieldRef<StingrayAI, StingrayHidingSpot> HidingSpot = AccessTools.FieldRefAccess<StingrayAI, StingrayHidingSpot>("hidingSpot");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StingrayAI), nameof(StingrayAI.Update))]
	public static void UpdatePrefix(StingrayAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead)
		{
			return;
		}
		__instance.targetPlayer = null;
		if (StingrayController.WantsCloaked)
		{
			TimeSpentInState(__instance) = 0f;
			TimeSinceHitting(__instance) = 15f;
			WatchPlayerSpitTimer(__instance) = 0f;
		}
		if (!__instance.IsOwner)
		{
			return;
		}
		if (__instance.currentBehaviourStateIndex != (int)StingrayState.ROAMING || PreviousBehaviourState(__instance) == (int)StingrayState.ROAMING)
		{
			return;
		}
		if (GameNetworkManager.Instance?.localPlayerController == null || GameNetworkManager.Instance.localPlayerController.playerClientId == 0L)
		{
			return; // the host runs the game's own start of the mode
		}
		// The same setup the game does when it enters this mode.
		TimeSpentInState(__instance) = 0f;
		if (__instance.agent != null)
		{
			__instance.agent.speed = 0f;
			__instance.agent.acceleration = 30f;
		}
		if (__instance.creatureAnimator != null)
		{
			__instance.creatureAnimator.SetBool("cloaked", false);
		}
		HasSpit(__instance) = false;
		StingrayHidingSpot hidingSpot = HidingSpot(__instance);
		if (hidingSpot != null)
		{
			hidingSpot.gotHidingSpot = false;
			hidingSpot.choseTemporarySpot = false;
			hidingSpot.type = HidingSpotType.Temporary;
		}
		WatchPlayerSpitTimer(__instance) = 0f;
		PreviousBehaviourState(__instance) = (int)StingrayState.ROAMING;
	}
}
