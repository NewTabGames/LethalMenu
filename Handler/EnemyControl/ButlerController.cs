using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class ButlerController : IEnemyController<ButlerEnemyAI>, IController
{
	/// <summary>
	/// Murder mode on / off. Outside it the Butler only has a 1-in-20 chance to stab someone it bumps into, once
	/// every 10 seconds, and only its own AI ever switched it on, so a driven Butler could hardly hurt anyone.
	/// In murder mode it pulls its knife and stabs whoever it touches, like in the game.
	/// </summary>
	public void UsePrimarySkill(ButlerEnemyAI enemy)
	{
		enemy.SwitchToBehaviourState(enemy.currentBehaviourStateIndex == 2 ? 0 : 2);
	}

	private bool aiWasDriving;

	public void Update(ButlerEnemyAI enemy, bool isAIControlled)
	{
		if (isAIControlled && !aiWasDriving)
		{
			HandMurderModeToAI(enemy);
		}
		aiWasDriving = isAIControlled;
	}

	public void OnReleaseControl(ButlerEnemyAI enemy)
	{
		HandMurderModeToAI(enemy);
		aiWasDriving = false;
	}

	/// <summary>
	/// The game's murder-mode chase only runs on the machine of the player it's after, and only if that player owns
	/// it. Left in murder mode when you let go (F9 or F11), it stood still with its knife out forever. Hand it to a
	/// victim the way the game does, or put the knife away if nobody's around.
	/// </summary>
	private static void HandMurderModeToAI(ButlerEnemyAI enemy)
	{
		if (enemy == null || enemy.isEnemyDead || !enemy.IsOwner || enemy.currentBehaviourStateIndex != 2)
		{
			return;
		}
		PlayerControllerB? target = Control.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 60f, requireLineOfSight: false);
		if (target != null)
		{
			enemy.SwitchOwnershipAndSetToStateServerRpc(2, target.actualClientId, 3f);
		}
		else
		{
			enemy.SwitchToBehaviourState(0);
		}
	}

	// Sweeping used to only ever switch on (and the game kept flipping it on and off by itself every few
	// seconds anyway). Now right click starts and stops it, and it stays how you left it.
	public void UseSecondarySkill(ButlerEnemyAI enemy)
	{
		if (!enemy.IsOwner)
		{
			return;
		}
		bool sweeping = !enemy.Reflect().GetValue<bool>("isSweeping");
		enemy.Reflect().SetValue("isSweeping", sweeping);
		enemy.SetSweepingAnimServerRpc(sweeping);
	}

	public string GetPrimarySkillName(ButlerEnemyAI enemy)
	{
		return enemy.currentBehaviourStateIndex == 2 ? "Put the knife away" : "Murder mode (stabs on touch)";
	}

	public string GetSecondarySkillName(ButlerEnemyAI enemy)
	{
		return enemy.Reflect().GetValue<bool>("isSweeping") ? "Stop sweeping" : "Sweep";
	}

	public bool CanUseEntranceDoors(ButlerEnemyAI _)
	{
		return false;
	}

	public float InteractRange(ButlerEnemyAI _)
	{
		return 5f;
	}
}

/// <summary>A driven Butler doesn't start or stop sweeping on its own timer or turn toward watched players.</summary>
[HarmonyPatch]
internal static class ButlerEnemyAIPatches
{
	private static readonly AccessTools.FieldRef<ButlerEnemyAI, float> SweepFloorTimer = AccessTools.FieldRefAccess<ButlerEnemyAI, float>("sweepFloorTimer");

	private static readonly AccessTools.FieldRef<ButlerEnemyAI, float> PingAttentionTimer = AccessTools.FieldRefAccess<ButlerEnemyAI, float>("pingAttentionTimer");

	private static readonly AccessTools.FieldRef<ButlerEnemyAI, PlayerControllerB> WatchingPlayer = AccessTools.FieldRefAccess<ButlerEnemyAI, PlayerControllerB>("watchingPlayer");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.Update))]
	public static void Update(ButlerEnemyAI __instance)
	{
		if (ButlerEnemyAI.murderMusicAudio == null && __instance != null)
		{
			ButlerEnemyAI.murderMusicAudio = __instance.ambience2;
		}
		if (Control.IsDrivenByYou(__instance))
		{
			SweepFloorTimer(__instance) = 10f;
			PingAttentionTimer(__instance) = -1f;
			WatchingPlayer(__instance) = null;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.CheckLOS))]
	public static bool CheckLOS(ButlerEnemyAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}
}
