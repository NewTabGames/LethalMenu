using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Cheats;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

[HarmonyPatch]
public class DressGirlAIPatches
{
	// Only for the Ghost Girl you're driving. This used to check just "is the AI driving", which is false
	// whenever you aren't controlling anything, so every Ghost Girl in the game was frozen as long as the menu
	// was loaded (she never haunted or chased anyone on the host).
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "Update")]
	public static bool Update(DressGirlAI __instance)
	{
		return !global::LethalMenu.Cheats.EnemyControl.IsDrivenByYou(__instance);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
	public static bool OnCollideWithPlayer(DressGirlAI __instance, Collider other)
	{
		if (global::LethalMenu.Cheats.EnemyControl.IsDrivenByYou(__instance))
		{
			PlayerControllerB closestPlayer = DressGirlController.GetClosestPlayer(__instance);
			if (closestPlayer != null && __instance.hauntingPlayer != null && other.gameObject == __instance.hauntingPlayer.gameObject && __instance.currentBehaviourStateIndex == 1)
			{
				closestPlayer.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 1, default(Vector3));
				__instance.Reflect().Invoke("StopChasing");
				((EnemyAI)__instance).EnableEnemyMesh(false, true);
				__instance.creatureSFX.Stop();
			}
			return false;
		}
		return true;
	}
}
