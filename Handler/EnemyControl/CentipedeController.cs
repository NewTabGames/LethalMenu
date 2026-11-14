using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class CentipedeController : IEnemyController<CentipedeAI>, IController
{
	private bool IsClingingToSomething(CentipedeAI enemy)
	{
		ReflectionUtil<CentipedeAI> reflectionUtil = enemy.Reflect();
		return (object)enemy.clingingToPlayer != null || enemy.inSpecialAnimation || reflectionUtil.GetValue<bool>("clingingToDeadBody") || reflectionUtil.GetValue<bool>("clingingToCeiling") || reflectionUtil.GetValue<bool>("startedCeilingAnimationCoroutine") || reflectionUtil.GetValue<bool>("inDroppingOffPlayerAnim");
	}

	public void UsePrimarySkill(CentipedeAI enemy)
	{
		if (enemy.IsBehaviourState(CentipedeState.HIDING))
		{
			enemy.SetBehaviourState(CentipedeState.CHASING);
		}
	}

	public void UseSecondarySkill(CentipedeAI enemy)
	{
		if (!IsClingingToSomething(enemy))
		{
			enemy.Reflect().Invoke("RaycastToCeiling");
			enemy.SetBehaviourState(CentipedeState.HIDING);
		}
	}

	public bool IsAbleToMove(CentipedeAI enemy)
	{
		return !IsClingingToSomething(enemy);
	}

	public bool IsAbleToRotate(CentipedeAI enemy)
	{
		return !IsClingingToSomething(enemy);
	}

	public string GetPrimarySkillName(CentipedeAI _)
	{
		return "Drop";
	}

	public string GetSecondarySkillName(CentipedeAI _)
	{
		return "Attach to ceiling";
	}

	public float InteractRange(CentipedeAI _)
	{
		return 1.5f;
	}

	public bool CanUseEntranceDoors(CentipedeAI _)
	{
		return false;
	}

	public bool SyncAnimationSpeedEnabled(CentipedeAI _)
	{
		return false;
	}
}
