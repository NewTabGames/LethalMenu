using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
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
	public void OnTakeControl(PumaAI enemy)
	{
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
		// The puma only damages on contact shortly after a scratch animation event; hold that window open.
		enemy.Reflect().SetValue("timeAtLastScratch", Time.realtimeSinceStartup);
	}

	public bool IsAbleToMove(PumaAI enemy)
	{
		return !enemy.inSpecialAnimation;
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
		CalculateAnimationDirection?.Invoke(__instance, new object[1] { 1f });
	}
}
