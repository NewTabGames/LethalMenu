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

	// Sweeping used to only ever switch on (and the game kept flipping it on and off by itself every few
	// seconds anyway). Now right click starts and stops it, and it stays how you left it.
	public void UseSecondarySkill(ButlerEnemyAI enemy)
	{
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

/// <summary>A driven Butler doesn't start or stop sweeping on its own timer.</summary>
[HarmonyPatch]
internal static class ButlerEnemyAIPatches
{
	private static readonly AccessTools.FieldRef<ButlerEnemyAI, float> SweepFloorTimer = AccessTools.FieldRefAccess<ButlerEnemyAI, float>("sweepFloorTimer");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.Update))]
	public static void Update(ButlerEnemyAI __instance)
	{
		if (Control.IsDrivenByYou(__instance))
		{
			SweepFloorTimer(__instance) = 10f;
		}
	}
}
