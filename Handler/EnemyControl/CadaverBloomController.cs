namespace LethalMenu.Handler.EnemyControl;

internal enum CadaverBloomState
{
	SEARCHING,
	CHASING,
	BURSTING
}

internal class CadaverBloomController : IEnemyController<CadaverBloomAI>, IController
{
	private bool chomping;

	public void OnMovement(CadaverBloomAI enemy, bool isMoving, bool isSprinting)
	{
		if (!enemy.IsBehaviourState(CadaverBloomState.BURSTING))
		{
			enemy.SetBehaviourState(isSprinting ? CadaverBloomState.CHASING : CadaverBloomState.SEARCHING);
		}
	}

	public void OnSecondarySkillHold(CadaverBloomAI enemy)
	{
		if (!chomping)
		{
			chomping = true;
			enemy.SyncChompingAnimRpc(chomping: true);
		}
	}

	public void ReleaseSecondarySkill(CadaverBloomAI enemy)
	{
		StopChomping(enemy);
	}

	public void OnReleaseControl(CadaverBloomAI enemy)
	{
		StopChomping(enemy);
	}

	private void StopChomping(CadaverBloomAI enemy)
	{
		if (chomping)
		{
			chomping = false;
			enemy.SyncChompingAnimRpc(chomping: false);
		}
	}

	public bool IsAbleToMove(CadaverBloomAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(CadaverBloomAI _)
	{
		return "Walk into players to bite";
	}

	public string GetSecondarySkillName(CadaverBloomAI _)
	{
		return "(HOLD) Chomp";
	}
}
