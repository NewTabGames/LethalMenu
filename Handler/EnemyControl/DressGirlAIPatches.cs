using HarmonyLib;
using LethalMenu.Cheats;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

[HarmonyPatch]
public class DressGirlAIPatches
{
	private static float nextSync;

	private static float lastSentYaw = float.NaN;

	// Only for the Ghost Girl you're driving. This used to check just "is the AI driving", which is false
	// whenever you aren't controlling anything, so every Ghost Girl in the game was frozen as long as the menu
	// was loaded (she never haunted or chased anyone on the host).
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "Update")]
	public static bool Update(DressGirlAI __instance)
	{
		if (!global::LethalMenu.Cheats.EnemyControl.IsDrivenByYou(__instance))
		{
			lastSentYaw = float.NaN;
			return true;
		}
		// Skipping her Update also skips the base EnemyAI.Update, which is what sends her position to the other
		// players. Everyone else kept her where she was when you took over, so she could never touch anyone.
		if (Time.time >= nextSync)
		{
			nextSync = Time.time + 0.1f;
			__instance.SyncPositionToClients();
			float yaw = __instance.transform.eulerAngles.y;
			if (float.IsNaN(lastSentYaw) || Mathf.Abs(Mathf.DeltaAngle(lastSentYaw, yaw)) > 6f)
			{
				lastSentYaw = yaw;
				__instance.Reflect().Invoke("UpdateEnemyRotationServerRpc", (short)yaw);
			}
		}
		// Stuns wear off in the base Update too.
		if (__instance.stunnedIndefinitely <= 0 && __instance.stunNormalizedTimer >= 0f)
		{
			__instance.stunNormalizedTimer -= Time.deltaTime / __instance.enemyType.stunTimeMultiplier;
		}
		return false;
	}

	// The kill happens on the haunted player's own game (her normal OnCollideWithPlayer there). From your side
	// KillPlayer on someone else does nothing, so just keep her own collision code out of the way here.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
	public static bool OnCollideWithPlayer(DressGirlAI __instance, Collider other)
	{
		return !global::LethalMenu.Cheats.EnemyControl.IsDrivenByYou(__instance);
	}
}
