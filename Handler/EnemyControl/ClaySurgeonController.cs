namespace LethalMenu.Handler.EnemyControl;

internal class ClaySurgeonController : IEnemyController<ClaySurgeonAI>, IController
{
	public void OnTakeControl(ClaySurgeonAI enemy)
	{
		enemy.SyncMasterClaySurgeonClientRpc();
	}

	public string GetPrimarySkillName(ClaySurgeonAI _)
	{
		return "";
	}

	public string GetSecondarySkillName(ClaySurgeonAI _)
	{
		return "";
	}

	public void OnMovement(ClaySurgeonAI enemy, bool isMoving, bool isSprinting)
	{
		if (!isSprinting && isMoving && enemy != null && IsJumpingAndSnipping(enemy))
		{
		}
	}

	public bool IsJumpingAndSnipping(ClaySurgeonAI enemy)
	{
		return enemy.isJumping && (double)enemy.agent.speed > 0.0;
	}

	public bool CanUseEntranceDoors(ClaySurgeonAI _)
	{
		return false;
	}

	public float InteractRange(ClaySurgeonAI _)
	{
		return 2.5f;
	}
}
