using HarmonyLib;
using LethalMenu.Util;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class ButlerController : IEnemyController<ButlerEnemyAI>, IController
{
	public void UsePrimarySkill(ButlerEnemyAI enemy)
	{
		enemy.SyncSearchingMadlyServerRpc(isSearching: true);
	}

	// Sweeping used to only ever switch on (and the game kept flipping it on and off by itself every few
	// seconds anyway). Now right click starts and stops it, and it stays how you left it.
	public void UseSecondarySkill(ButlerEnemyAI enemy)
	{
		bool sweeping = !enemy.Reflect().GetValue<bool>("isSweeping");
		enemy.Reflect().SetValue("isSweeping", sweeping);
		enemy.SetSweepingAnimServerRpc(sweeping);
	}

	public string GetPrimarySkillName(ButlerEnemyAI _)
	{
		return "Search Mad";
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
