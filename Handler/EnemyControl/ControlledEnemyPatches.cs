using HarmonyLib;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>Rules for every monster you drive.</summary>
[HarmonyPatch]
internal static class ControlledEnemyPatches
{
	// Some monsters hand themselves to another player by their own AI (the Feiopar gives itself back to the host,
	// or to the player it stalks), which took a monster away from you mid-control. Not while you drive it.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.ChangeOwnershipOfEnemy))]
	public static bool ChangeOwnershipOfEnemy(EnemyAI __instance, ulong newOwnerClientId)
	{
		return !Control.IsDrivenByYou(__instance) || LethalMenu.localPlayer == null || newOwnerClientId == LethalMenu.localPlayer.actualClientId;
	}
}
