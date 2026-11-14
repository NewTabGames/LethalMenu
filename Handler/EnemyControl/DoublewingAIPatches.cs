using HarmonyLib;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>Stops a manticoil's own AI from fighting you while you drive it, and points its body where you look.</summary>
[HarmonyPatch]
internal static class DoublewingAIPatches
{
	// Its AI would take off by itself whenever it sees or hears a player, then try to land somewhere else.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.DoAIInterval))]
	public static bool DoAIInterval(DoublewingAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance))
		{
			return true;
		}
		// Skipping its AI also skips the game's "send my position to the other players" step, so do that part.
		__instance.SyncPositionToClients();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.AlertBird))]
	public static bool AlertBird(DoublewingAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.AlertBirdByOther))]
	public static bool AlertBirdByOther(DoublewingAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
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
