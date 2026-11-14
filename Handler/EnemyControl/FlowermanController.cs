namespace LethalMenu.Handler.EnemyControl;

internal class FlowermanController : IEnemyController<FlowermanAI>, IController
{
	public void UsePrimarySkill(FlowermanAI enemy)
	{
		if (enemy.carryingPlayerBody)
		{
			enemy.DropPlayerBodyServerRpc();
			return;
		}
		enemy.EnterAngerModeServerRpc(12f);
		enemy.SetBehaviourState(FlowermanState.ANGER);
	}

	public void UseSecondarySkill(FlowermanAI enemy)
	{
		enemy.SetBehaviourState(FlowermanState.STAND);
	}

	public void ReleaseSecondarySkill(FlowermanAI enemy)
	{
		enemy.SetBehaviourState(FlowermanState.SCOUTING);
	}

	public bool IsAbleToMove(FlowermanAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(FlowermanAI enemy)
	{
		return enemy.carryingPlayerBody ? "Drop body" : "Get angry";
	}

	public string GetSecondarySkillName(FlowermanAI _)
	{
		return "(HOLD) Stand up";
	}

	public float InteractRange(FlowermanAI _)
	{
		return 1.5f;
	}

	public bool SyncAnimationSpeedEnabled(FlowermanAI _)
	{
		return false;
	}

	public bool CanUseEntranceDoors(FlowermanAI _)
	{
		return false;
	}
}
