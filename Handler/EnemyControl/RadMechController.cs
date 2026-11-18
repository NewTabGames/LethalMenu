using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Old Bird. Walks with its stomping steps only while you move, and left click fires a missile where you aim
/// (host only: the game lets only the host fire it for everyone).
/// </summary>
internal class RadMechController : IEnemyController<RadMechAI>, IController
{
	private const float FireCooldown = 0.6f;

	/// <summary>How long the gun arm stays raised after a shot.</summary>
	private const float AimHoldSeconds = 1.2f;

	private float nextShot;

	private float lowerGunAt;

	public void OnTakeControl(RadMechAI enemy)
	{
		nextShot = 0f;
		lowerGunAt = 0f;
		// Out of its hunting mode (spotlight, aiming at targets); it isn't sent flying off while driven either.
		// Any player may ask for the switch; the host then turns the spotlight and gun off for everyone. (A client
		// driver used to leave it hunting, still firing at its old target.)
		if (enemy.currentBehaviourStateIndex == 1)
		{
			enemy.SwitchToBehaviourState(0);
		}
	}

	public void OnReleaseControl(RadMechAI enemy)
	{
		if (enemy != null && enemy.creatureAnimator != null)
		{
			enemy.creatureAnimator.SetBool("AimGun", false);
		}
		if (enemy != null)
		{
			RestoreStepGap(enemy);
		}
	}

	/// <summary>
	/// Driven, it steps with almost no pause (see the patch below). Its patrol sets its normal pause only when
	/// it starts patrolling, so handed back mid-patrol it kept marching about 14 times too fast.
	/// </summary>
	private static void RestoreStepGap(RadMechAI enemy)
	{
		if (enemy.currentBehaviourStateIndex == 0 && enemy.timeBetweenSteps < 0.7f)
		{
			enemy.timeBetweenSteps = 0.7f;
		}
	}

	public void Update(RadMechAI enemy, bool isAIControlled)
	{
		if (isAIControlled)
		{
			RestoreStepGap(enemy); // F9: its AI walks it again
		}
		if (lowerGunAt > 0f && Time.time >= lowerGunAt && enemy.creatureAnimator != null)
		{
			lowerGunAt = 0f;
			enemy.creatureAnimator.SetBool("AimGun", false);
		}
	}

	public string GetPrimarySkillName(RadMechAI _)
	{
		return "Fire missile";
	}

	public void UsePrimarySkill(RadMechAI enemy)
	{
		if (!enemy.IsServer)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Only the host can fire the Old Bird's gun");
			return;
		}
		if (Time.time < nextShot || enemy.gunPoint == null)
		{
			return;
		}
		nextShot = Time.time + FireCooldown;
		Control.EnsureMapPropsContainer();
		Vector3 from = enemy.gunPoint.position;
		Vector3 direction = Control.AimPoint(120f) - from;
		if (direction.sqrMagnitude < 0.01f)
		{
			direction = enemy.transform.forward;
		}
		Vector3 rotation = Quaternion.LookRotation(direction).eulerAngles;
		enemy.creatureAnimator.SetBool("AimGun", true); // so the shot plays its recoil
		lowerGunAt = Time.time + AimHoldSeconds;
		enemy.ShootGun(from, rotation);
		enemy.ShootGunClientRpc(from, rotation);
	}

	public string GetSecondarySkillName(RadMechAI _)
	{
		return "";
	}

	public bool CanUseEntranceDoors(RadMechAI _)
	{
		return false;
	}

	public float InteractRange(RadMechAI _)
	{
		return 10f;
	}
}

/// <summary>Keeps a driven Old Bird from flying off or hunting by itself, and steps only while you move.</summary>
[HarmonyPatch]
internal static class RadMechAIPatches
{
	private static readonly AccessTools.FieldRef<RadMechAI, bool> TakingStep = AccessTools.FieldRefAccess<RadMechAI, bool>("takingStep");

	private static readonly AccessTools.FieldRef<RadMechAI, bool> WaitingToAttemptGrab = AccessTools.FieldRefAccess<RadMechAI, bool>("waitingToAttemptGrab");

	private static readonly AccessTools.FieldRef<RadMechAI, bool> AttemptingGrab = AccessTools.FieldRefAccess<RadMechAI, bool>("attemptingGrab");

	// Its walk is a series of steps the game starts on a timer, whether it goes anywhere or not: it marched on
	// the spot while standing still, and paused between steps while you moved smoothly. Driven, it only steps
	// while you move, one step straight after the other.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(RadMechAI), "DoFootstepCycle")]
	public static bool DoFootstepCycle(RadMechAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance))
		{
			return true;
		}
		if (__instance.chargingForward)
		{
			__instance.SetChargingForwardOnLocalClient(false);
		}
		if (!Control.YouAreMoving)
		{
			TakingStep(__instance) = false;
			return false;
		}
		__instance.timeBetweenSteps = 0.05f;
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.AttemptGrabIfClose))]
	public static bool AttemptGrabIfClose(RadMechAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || WaitingToAttemptGrab(__instance) || AttemptingGrab(__instance) || StartOfRound.Instance?.allPlayerScripts == null)
		{
			return true;
		}
		foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
		{
			if (player == null || !player.isPlayerControlled || player.isPlayerDead)
			{
				continue;
			}
			if (!Control.TargetingYourself && (object)player == LethalMenu.localPlayer)
			{
				continue;
			}
			if (Vector3.Distance(__instance.transform.position, player.transform.position) < 5.2f)
			{
				return true;
			}
		}
		return false;
	}
}
