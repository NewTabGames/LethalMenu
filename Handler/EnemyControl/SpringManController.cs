using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class SpringManController : IEnemyController<SpringManAI>, IController
{
	private bool GetStoppingMovement(SpringManAI enemy)
	{
		return enemy.Reflect().GetValue<bool>("stoppingMovement");
	}

	public void UseSecondarySkill(SpringManAI enemy)
	{
		enemy.SetAnimationGoServerRpc();
	}

	public void ReleaseSecondarySkill(SpringManAI enemy)
	{
		enemy.SetAnimationStopServerRpc();
	}

	public bool IsAbleToMove(SpringManAI enemy)
	{
		return !GetStoppingMovement(enemy);
	}

	public bool IsAbleToRotate(SpringManAI enemy)
	{
		return !GetStoppingMovement(enemy);
	}

	public float InteractRange(SpringManAI _)
	{
		return 1.5f;
	}

	public bool CanUseEntranceDoors(SpringManAI _)
	{
		return false;
	}

	public string GetSecondarySkillName(SpringManAI _)
	{
		return "(HOLD) Move (it freezes again when you let go)";
	}
}
