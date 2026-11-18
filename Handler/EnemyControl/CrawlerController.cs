using HarmonyLib;

namespace LethalMenu.Handler.EnemyControl;

internal enum CrawlerState
{
	SEARCHING,
	CHASING
}

internal class CrawlerController : IEnemyController<CrawlerAI>, IController
{
	private static readonly AccessTools.FieldRef<CrawlerAI, float> CheckLineOfSightInterval = AccessTools.FieldRefAccess<CrawlerAI, float>("checkLineOfSightInterval");

	public void Update(CrawlerAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null)
		{
			return;
		}
		CheckLineOfSightInterval(enemy) = 0f;
		enemy.noticePlayerTimer = 0f;
	}

	public void OnMovement(CrawlerAI enemy, bool isMoving, bool isSprinting)
	{
		if (!isSprinting)
		{
			if (isMoving)
			{
				enemy.SetBehaviourState(CrawlerState.SEARCHING);
			}
		}
		else
		{
			enemy.SetBehaviourState(CrawlerState.CHASING);
		}
	}

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
