namespace LethalMenu.Handler.EnemyControl;

internal class RedLocustBeesController : IEnemyController<RedLocustBees>, IController
{
	public bool CanUseEntranceDoors(RedLocustBees _)
	{
		return false;
	}

	public float InteractRange(RedLocustBees _)
	{
		return 2.5f;
	}

	public void UsePrimarySkill(RedLocustBees enemy)
	{
		enemy.SetBehaviourState(BeesState.ATTACK);
		enemy.EnterAttackZapModeServerRpc(-1);
	}

	public void UseSecondarySkill(RedLocustBees enemy)
	{
		enemy.SetBehaviourState(BeesState.IDLE);
	}

	public string GetPrimarySkillName(RedLocustBees _)
	{
		return "Attack (zap)";
	}

	public string GetSecondarySkillName(RedLocustBees _)
	{
		return "Calm down";
	}
}
