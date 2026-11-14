namespace LethalMenu.Handler.EnemyControl;

internal class LassoManController : IEnemyController<LassoManAI>, IController
{
	public void UsePrimarySkill(LassoManAI enemy)
	{
		enemy.MakeScreechNoiseServerRpc();
	}

	public bool SyncAnimationSpeedEnabled(LassoManAI enemy)
	{
		return false;
	}

	public bool CanUseEntranceDoors(LassoManAI _)
	{
		return true;
	}

	public string GetPrimarySkillName(LassoManAI _)
	{
		return "Screech";
	}
}
