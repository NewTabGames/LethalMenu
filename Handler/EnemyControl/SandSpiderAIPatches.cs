using HarmonyLib;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Keeps a controlled Bunker Spider's body with you.
///
/// The spider's visible body (its "mesh container") is a separate object that its own AI walks along behind the
/// real position, and sends up walls and ceilings to wait for prey. While you drove the spider its AI kept doing
/// that: the camera followed the real position you steer while the body stayed on a wall, stood still or crawled
/// off somewhere else, so the spider seemed to vanish and the camera seemed to race ahead of it.
/// </summary>
[HarmonyPatch]
internal static class SandSpiderAIPatches
{
	private static readonly AccessTools.FieldRef<SandSpiderAI, bool> OnWall = AccessTools.FieldRefAccess<SandSpiderAI, bool>("onWall");

	private static readonly AccessTools.FieldRef<SandSpiderAI, bool> GotWallPosition = AccessTools.FieldRefAccess<SandSpiderAI, bool>("gotWallPositionInLOS");

	private static readonly AccessTools.FieldRef<SandSpiderAI, bool> ReachedWall = AccessTools.FieldRefAccess<SandSpiderAI, bool>("reachedWallPosition");

	private static readonly AccessTools.FieldRef<SandSpiderAI, bool> OverrideLook = AccessTools.FieldRefAccess<SandSpiderAI, bool>("overrideSpiderLookRotation");

	private static readonly AccessTools.FieldRef<SandSpiderAI, Quaternion> TargetRotation = AccessTools.FieldRefAccess<SandSpiderAI, Quaternion>("meshContainerTargetRotation");

	private static readonly AccessTools.FieldRef<SandSpiderAI, float> TimeSincePlacingWebTrap = AccessTools.FieldRefAccess<SandSpiderAI, float>("timeSincePlacingWebTrap");

	private static readonly AccessTools.FieldRef<SandSpiderAI, bool> WatchFromDistance = AccessTools.FieldRefAccess<SandSpiderAI, bool>("watchFromDistance");

	/// <summary>Farther than this from where you are and the body jumps straight there (teleports, entrances).</summary>
	private const float SnapDistance = 4f;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
	public static void Update(SandSpiderAI __instance)
	{
		if (Control.IsDrivenByYou(__instance))
		{
			TimeSincePlacingWebTrap(__instance) = 0f;
			WatchFromDistance(__instance) = false;
		}
	}

	// Runs right before the game moves the body each frame: aim it at where you are, facing where you face.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.LateUpdate))]
	public static void LateUpdate(SandSpiderAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead || __instance.meshContainer == null)
		{
			return;
		}
		__instance.lookingForWallPosition = false;
		GotWallPosition(__instance) = false;
		ReachedWall(__instance) = false;
		OnWall(__instance) = false;
		OverrideLook(__instance) = false;
		__instance.navigateMeshTowardsPosition = true; // stops its own route planning from moving the target
		Vector3 here = __instance.transform.position;
		float distance = Vector3.Distance(__instance.meshContainerPosition, here);
		if (distance > SnapDistance)
		{
			__instance.meshContainerPosition = here;
		}
		__instance.meshContainerTarget = here;
		// Fast enough to stay right under you at any speed, still smooth over small steps.
		__instance.spiderSpeed = Mathf.Max(8f, distance * 12f);
		TargetRotation(__instance) = Quaternion.Euler(0f, __instance.transform.eulerAngles.y, 0f);
	}
}
