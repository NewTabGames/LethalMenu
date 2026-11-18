using HarmonyLib;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class NutcrackerController : IEnemyController<NutcrackerEnemyAI>, IController
{
	private static readonly AccessTools.FieldRef<NutcrackerEnemyAI, float> TimeSinceInspecting = AccessTools.FieldRefAccess<NutcrackerEnemyAI, float>("timeSinceInspecting");

	private bool InSentryMode { get; set; } = false;

	public void Update(NutcrackerEnemyAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null)
		{
			return;
		}
		if (!InSentryMode)
		{
			// Its patrol drops into a look-around every few seconds by itself (in its Update, which still runs while
			// you drive): it froze you and played its alert sound. Keep that timer from ever running out.
			TimeSinceInspecting(enemy) = 0f;
			enemy.SetBehaviourState(NutcrackerState.WALKING);
		}
		else if (!enemy.IsBehaviourState(NutcrackerState.SENTRY))
		{
			enemy.SetBehaviourState(NutcrackerState.SENTRY);
		}
	}

	public bool IsAbleToRotate(NutcrackerEnemyAI enemy)
	{
		return !enemy.IsBehaviourState(NutcrackerState.SENTRY);
	}

	public bool IsAbleToMove(NutcrackerEnemyAI enemy)
	{
		return !enemy.IsBehaviourState(NutcrackerState.SENTRY);
	}

	public void UsePrimarySkill(NutcrackerEnemyAI enemy)
	{
		// The volume used to be turned down here on the real shotgun and never put back, so every later shot from
		// that gun (its AI, or a player who picked it up) stayed quiet.
		if (enemy.IsOwner && (object)enemy.gun != null)
		{
			enemy.FireGunServerRpc();
		}
	}

	public void OnSecondarySkillHold(NutcrackerEnemyAI enemy)
	{
		enemy.SetBehaviourState(NutcrackerState.SENTRY);
		InSentryMode = true;
	}

	public void ReleaseSecondarySkill(NutcrackerEnemyAI enemy)
	{
		enemy.SetBehaviourState(NutcrackerState.WALKING);
		InSentryMode = false;
	}

	public void OnReleaseControl(NutcrackerEnemyAI enemy)
	{
		InSentryMode = false;
	}

	public string GetPrimarySkillName(NutcrackerEnemyAI enemy)
	{
		return ((object)enemy.gun == null) ? "" : "Fire";
	}

	public string GetSecondarySkillName(NutcrackerEnemyAI _)
	{
		return "(HOLD) Sentry mode";
	}

	public float InteractRange(NutcrackerEnemyAI _)
	{
		return 1.5f;
	}

	public bool CanUseEntranceDoors(NutcrackerEnemyAI _)
	{
		return false;
	}
}

[HarmonyPatch]
internal static class NutcrackerAIPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.SeeMovingThreatServerRpc))]
	public static bool SeeMovingThreatServerRpc(NutcrackerEnemyAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}
}
