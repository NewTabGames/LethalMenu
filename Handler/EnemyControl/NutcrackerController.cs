namespace LethalMenu.Handler.EnemyControl;

internal class NutcrackerController : IEnemyController<NutcrackerEnemyAI>, IController
{
	private bool InSentryMode { get; set; } = false;

	public void Update(NutcrackerEnemyAI enemy, bool isAIControlled)
	{
		if (!isAIControlled && !InSentryMode)
		{
			enemy.SetBehaviourState(NutcrackerState.WALKING);
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
		ShotgunItem gun = enemy.gun;
		if ((object)gun != null)
		{
			gun.gunShootAudio.volume = 0.25f;
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
