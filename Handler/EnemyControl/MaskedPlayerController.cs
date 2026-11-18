using HarmonyLib;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class MaskedPlayerController : IEnemyController<MaskedPlayerEnemy>, IController
{
	private static readonly AccessTools.FieldRef<MaskedPlayerEnemy, bool> Crouching = AccessTools.FieldRefAccess<MaskedPlayerEnemy, bool>("crouching");

	private static readonly AccessTools.FieldRef<MaskedPlayerEnemy, bool> Running = AccessTools.FieldRefAccess<MaskedPlayerEnemy, bool>("running");

	private static readonly AccessTools.FieldRef<MaskedPlayerEnemy, bool> HandsOut = AccessTools.FieldRefAccess<MaskedPlayerEnemy, bool>("handsOut");

	private static readonly AccessTools.FieldRef<MaskedPlayerEnemy, bool> InKillAnimation = AccessTools.FieldRefAccess<MaskedPlayerEnemy, bool>("inKillAnimation");

	internal static bool CrouchToggled;

	private static RuntimeAnimatorController? originalController;

	private static AnimatorOverrideController? overrideController;

	private static AnimationClip? originalCrouchClip;

	private static AnimationClip? currentOverrideClip;

	private static AnimationClip? crouchIdleClip;

	private static AnimationClip? crouchWalkClip;

	private static AnimationClip? jumpClip;

	private static AnimationClip? jumpLandClip;

	private static AnimationClip? climbLadderClip;

	private static bool wasAirborne;

	private static float landTimer;

	private static bool wasPlayingCustomClip;

	public void OnTakeControl(MaskedPlayerEnemy enemy)
	{
		CrouchToggled = enemy != null && Crouching(enemy);
		wasAirborne = false;
		landTimer = 0f;
		wasPlayingCustomClip = false;
		currentOverrideClip = null;
		if (enemy != null)
		{
			SetupOverrideController(enemy);
		}
	}

	public void Update(MaskedPlayerEnemy enemy, bool isAIControlled)
	{
		if (isAIControlled)
		{
			if (CrouchToggled || wasPlayingCustomClip)
			{
				CrouchToggled = false;
				Control.SetCrouching(crouching: false);
				RestoreAnimator(enemy);
			}
			return;
		}
		if (Control.YouAreSprinting)
		{
			CrouchToggled = false;
		}
		bool wantCrouch = IsWantingCrouch();
		Control.SetCrouching(wantCrouch);
		SyncCrouching(enemy, wantCrouch);
	}

	// Its AI is what normally switches the run animation on, and that is paused while you drive it, so it slid
	// along at sprint speed with the walk animation.
	public void OnMovement(MaskedPlayerEnemy enemy, bool isMoving, bool isSprinting)
	{
		SetRunning(enemy, isMoving && isSprinting && !IsWantingCrouch() && !Control.IsOnLadder);
	}

	public void OnReleaseControl(MaskedPlayerEnemy enemy)
	{
		CrouchToggled = false;
		Control.SetCrouching(crouching: false);
		if (enemy != null)
		{
			RestoreAnimator(enemy);
			SetRunning(enemy, run: false);
			SyncCrouching(enemy, crouch: false);
		}
	}

	internal static bool IsWantingCrouch()
	{
		return (CrouchToggled || Control.IsSneaking) && !Control.YouAreSprinting && !Control.IsOnLadder && Control.IsOnGround;
	}

	private static void SetRunning(MaskedPlayerEnemy enemy, bool run)
	{
		if (!enemy.IsOwner || enemy.creatureAnimator == null || (Running(enemy) == run && enemy.creatureAnimator.GetBool("Running") == run))
		{
			return;
		}
		Running(enemy) = run;
		enemy.creatureAnimator.SetBool("Running", run);
		enemy.SetRunningServerRpc(run);
	}

	private static void SyncCrouching(MaskedPlayerEnemy enemy, bool crouch)
	{
		if (!enemy.IsOwner || Crouching(enemy) == crouch)
		{
			return;
		}
		Crouching(enemy) = crouch;
		enemy.SetCrouchingServerRpc(crouch);
	}

	private static void EnsurePlayerClipsCached()
	{
		if (crouchWalkClip != null && jumpClip != null && climbLadderClip != null)
		{
			return;
		}
		RuntimeAnimatorController? source = StartOfRound.Instance?.otherClientsAnimatorController
			?? StartOfRound.Instance?.localClientAnimatorController
			?? LethalMenu.localPlayer?.playerBodyAnimator?.runtimeAnimatorController;
		if (source == null)
		{
			return;
		}
		foreach (AnimationClip clip in source.animationClips)
		{
			if (clip == null)
			{
				continue;
			}
			switch (clip.name)
			{
			case "CrouchIdle":
				crouchIdleClip = clip;
				break;
			case "CrouchWalk":
				crouchWalkClip = clip;
				break;
			case "Jump":
				jumpClip = clip;
				break;
			case "JumpLand":
				jumpLandClip = clip;
				break;
			case "ClimbLadder":
				climbLadderClip = clip;
				break;
			}
		}
	}

	private static void SetupOverrideController(MaskedPlayerEnemy enemy)
	{
		Animator? animator = enemy.creatureAnimator;
		if (animator == null || animator.runtimeAnimatorController == null)
		{
			return;
		}
		EnsurePlayerClipsCached();
		if (animator.runtimeAnimatorController == overrideController && overrideController != null)
		{
			return;
		}
		RuntimeAnimatorController baseCtrl = animator.runtimeAnimatorController;
		if (baseCtrl is AnimatorOverrideController existingOverride && existingOverride.runtimeAnimatorController != null)
		{
			baseCtrl = existingOverride.runtimeAnimatorController;
		}
		originalController = baseCtrl;
		originalCrouchClip = null;
		foreach (AnimationClip clip in baseCtrl.animationClips)
		{
			if (clip != null && clip.name == "MaskedPlayerCrouch")
			{
				originalCrouchClip = clip;
				break;
			}
		}
		overrideController = new AnimatorOverrideController(baseCtrl);
		currentOverrideClip = originalCrouchClip;
		animator.runtimeAnimatorController = overrideController;
	}

	private static void RestoreAnimator(MaskedPlayerEnemy enemy)
	{
		wasAirborne = false;
		landTimer = 0f;
		wasPlayingCustomClip = false;
		currentOverrideClip = null;
		Animator? animator = enemy.creatureAnimator;
		if (animator != null)
		{
			animator.speed = 1f;
			if (originalController != null && animator.runtimeAnimatorController == overrideController)
			{
				animator.runtimeAnimatorController = originalController;
			}
			animator.SetBool("Crouching", value: false);
			animator.SetBool("Running", Running(enemy));
			animator.SetBool("HandsOut", HandsOut(enemy));
		}
		overrideController = null;
		originalController = null;
	}

	internal static void ApplyAnimations(MaskedPlayerEnemy enemy)
	{
		Animator? animator = enemy.creatureAnimator;
		if (animator == null || InKillAnimation(enemy) || enemy.inSpecialAnimation || enemy.stunNormalizedTimer >= 0f)
		{
			if (animator != null && animator.speed == 0f)
			{
				animator.speed = 1f;
			}
			return;
		}
		SetupOverrideController(enemy);
		if (overrideController == null)
		{
			return;
		}

		bool onLadder = Control.IsOnLadder;
		bool onGround = Control.IsOnGround;
		if (!onGround && !onLadder)
		{
			wasAirborne = true;
			landTimer = 0f;
		}
		else if (wasAirborne && onGround && !onLadder)
		{
			wasAirborne = false;
			landTimer = 0.22f;
		}
		else if (landTimer > 0f)
		{
			landTimer -= Time.deltaTime;
		}

		bool wantCrouch = IsWantingCrouch();
		AnimationClip? desiredClip = null;
		if (onLadder)
		{
			desiredClip = climbLadderClip;
		}
		else if (!onGround)
		{
			desiredClip = jumpClip;
		}
		else if (wantCrouch)
		{
			desiredClip = Control.YouAreMoving
				? (crouchWalkClip ?? crouchIdleClip ?? originalCrouchClip)
				: (crouchIdleClip ?? originalCrouchClip);
		}
		else if (landTimer > 0f && !Control.YouAreMoving)
		{
			desiredClip = jumpLandClip;
		}

		if (desiredClip != null)
		{
			if (currentOverrideClip != desiredClip)
			{
				overrideController["MaskedPlayerCrouch"] = desiredClip;
				currentOverrideClip = desiredClip;
				animator.SetBool("Running", Running(enemy));
				animator.SetBool("HandsOut", HandsOut(enemy));
				animator.SetBool("IsMoving", Control.YouAreMoving);
				animator.SetBool("Crouching", value: true);
				animator.Play("MaskedPlayerCrouch", 0, 0f);
			}
			else
			{
				animator.SetBool("Crouching", value: true);
				AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
				if (!state.IsName("MaskedPlayerCrouch") && !animator.IsInTransition(0))
				{
					animator.CrossFadeInFixedTime("MaskedPlayerCrouch", 0.08f);
				}
			}
			animator.speed = (onLadder && !Control.YouAreMoving) ? 0f : 1f;
			wasPlayingCustomClip = true;
		}
		else
		{
			animator.speed = 1f;
			animator.SetBool("Crouching", value: false);
			if (wasPlayingCustomClip)
			{
				wasPlayingCustomClip = false;
				animator.SetBool("Running", Running(enemy));
				animator.SetBool("HandsOut", HandsOut(enemy));
				string nextState = !Control.YouAreMoving
					? "Idle"
					: (Running(enemy)
						? (HandsOut(enemy) ? "RunArmsOut" : "Run")
						: (HandsOut(enemy) ? "WalkArmsOut 0" : "Walk"));
				animator.CrossFadeInFixedTime(nextState, 0.1f);
			}
		}
	}

	public void UsePrimarySkill(MaskedPlayerEnemy enemy)
	{
		bool next = !HandsOut(enemy);
		HandsOut(enemy) = next;
		enemy.creatureAnimator?.SetBool("HandsOut", next);
		if (enemy.IsOwner)
		{
			enemy.SetHandsOutServerRpc(next);
		}
	}

	public void UseSecondarySkill(MaskedPlayerEnemy enemy)
	{
		CrouchToggled = !CrouchToggled;
		bool wantCrouch = IsWantingCrouch();
		Control.SetCrouching(wantCrouch);
		SyncCrouching(enemy, wantCrouch);
	}

	public float InteractRange(MaskedPlayerEnemy _)
	{
		return 1f;
	}

	public bool SyncAnimationSpeedEnabled(MaskedPlayerEnemy _)
	{
		return false;
	}

	public bool CanUseEntranceDoors(MaskedPlayerEnemy _)
	{
		return true;
	}

	public string GetPrimarySkillName(MaskedPlayerEnemy enemy)
	{
		return HandsOut(enemy) ? "Lower hands" : "Raise hands";
	}

	public string GetSecondarySkillName(MaskedPlayerEnemy _)
	{
		return IsWantingCrouch() ? "Stand up" : "Crouch";
	}
}

[HarmonyPatch]
internal static class MaskedPlayerAIPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.Update))]
	public static void UpdatePostfix(MaskedPlayerEnemy __instance)
	{
		if (Control.IsDrivenByYou(__instance) && !__instance.isEnemyDead)
		{
			MaskedPlayerController.ApplyAnimations(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.SetCrouchingClientRpc))]
	public static void SetCrouchingClientRpcPostfix(MaskedPlayerEnemy __instance, bool setCrouch)
	{
		if (Control.IsDrivenByYou(__instance) || __instance.creatureAnimator == null)
		{
			return;
		}
		// The game's animator only crouches from walking, so a Masked someone else drives crouched only while
		// walking on your screen. Go into the crouch from any pose, and back out when it stands up (otherwise it
		// could stay crouched on your screen).
		if (setCrouch)
		{
			__instance.creatureAnimator.CrossFadeInFixedTime("MaskedPlayerCrouch", 0.1f);
		}
		else if (__instance.creatureAnimator.GetCurrentAnimatorStateInfo(0).IsName("MaskedPlayerCrouch"))
		{
			__instance.creatureAnimator.CrossFadeInFixedTime("Idle", 0.1f);
		}
	}
}
