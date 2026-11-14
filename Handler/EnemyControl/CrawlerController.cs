namespace LethalMenu.Handler.EnemyControl;

internal class CrawlerController : IEnemyController<CrawlerAI>, IController
{
	public float InteractRange(CrawlerAI _)
	{
		return 1.5f;
	}

	public bool SyncAnimationSpeedEnabled(CrawlerAI _)
	{
		return false;
	}

	public void UseSecondarySkill(CrawlerAI enemy)
	{
		enemy.MakeScreechNoiseServerRpc();
	}

	public void UsePrimarySkill(CrawlerAI enemy)
	{
		enemy.CollideWithWallServerRpc();
	}

	public bool CanUseEntranceDoors(CrawlerAI _)
	{
		return false;
	}

	public string GetPrimarySkillName(CrawlerAI _)
	{
		return "Slam into wall";
	}

	public string GetSecondarySkillName(CrawlerAI _)
	{
		return "Screech";
	}
}
