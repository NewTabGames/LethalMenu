using HarmonyLib;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>Stops a manticoil's own AI from fighting you while you drive it, and points its body where you look.</summary>
[HarmonyPatch]
internal static class DoublewingAIPatches
{
	private static readonly AccessTools.FieldRef<DoublewingAI, bool> AlertingBird = AccessTools.FieldRefAccess<DoublewingAI, bool>("alertingBird");

	// Noises you hear used to start an alert for everyone: the other players saw the bird take off while you
	// kept it on the ground. Your bird doesn't react to noise while you drive it.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.DetectNoise))]
	public static bool DetectNoise(DoublewingAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}

	// An alert started by another player still reaches every game. Skipping it here left alertingBird stuck on
	// (the bird ignored sights and noises after you let go), and the others still saw it fly off. Clear it and
	// send your real landed/flying state back to everyone.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.AlertBird))]
	public static bool AlertBird(DoublewingAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance))
		{
			return true;
		}
		AlertingBird(__instance) = false;
		__instance.SwitchToBehaviourServerRpc(__instance.currentBehaviourStateIndex);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.AlertBirdByOther))]
	public static bool AlertBirdByOther(DoublewingAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance))
		{
			return true;
		}
		// A bird next to it took off; the other players' games make yours take off too. Put them back.
		__instance.SwitchToBehaviourServerRpc(__instance.currentBehaviourStateIndex);
		return false;
	}

	// The game keeps a landed manticoil's body facing a fixed random direction, so turning right made it
	// swing left on screen. While you drive it, the body faces where you face (tilted up/down in flight).
	[HarmonyPostfix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.SetFlyDirection))]
	public static void SetFlyDirection(DoublewingAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.Body == null || __instance.isEnemyDead)
		{
			return;
		}
		float pitch = Control.IsFlying ? Mathf.Clamp(-Control.VerticalSpeed * 4f, -35f, 35f) : 0f;
		Quaternion facing = Quaternion.Euler(pitch, __instance.transform.eulerAngles.y, 0f);
		__instance.Body.rotation = Quaternion.Slerp(__instance.Body.rotation, facing, 12f * Time.deltaTime);
		// The game also raises the model with speed to fake flight; the real height now comes from flying,
		// so keep the model where the bird actually is.
		if (__instance.bodyAnimator != null)
		{
			__instance.bodyAnimator.SetFloat("elevation", 0.02f);
		}
	}
}
