using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Cheats;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

[HarmonyPatch]
public class DressGirlAIPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "Update")]
	public static bool Update(DressGirlAI __instance)
	{
		if (!global::LethalMenu.Cheats.EnemyControl.IsAIControlled)
		{
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
	public static bool OnCollideWithPlayer(DressGirlAI __instance, Collider other)
	{
		if (!global::LethalMenu.Cheats.EnemyControl.IsAIControlled)
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
