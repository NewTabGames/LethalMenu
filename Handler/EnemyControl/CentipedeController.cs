using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class CentipedeController : IEnemyController<CentipedeAI>, IController
{
	private bool IsClingingToSomething(CentipedeAI enemy)
	{
		ReflectionUtil<CentipedeAI> reflectionUtil = enemy.Reflect();
		return (object)enemy.clingingToPlayer != null || enemy.inSpecialAnimation || reflectionUtil.GetValue<bool>("clingingToDeadBody") || reflectionUtil.GetValue<bool>("clingingToCeiling") || reflectionUtil.GetValue<bool>("startedCeilingAnimationCoroutine") || reflectionUtil.GetValue<bool>("inDroppingOffPlayerAnim");
	}

	/// <summary>
	/// It only grabs onto someone's head in its chase mode. Before, the only way into it was dropping from the
	/// ceiling, so a Snare Flea walking on the ground could never attack. Now left click drops from the ceiling,
	/// or on the ground switches to chase mode (then touch a player to grab their head).
	/// </summary>
	public void UsePrimarySkill(CentipedeAI enemy)
	{
		if (!enemy.IsBehaviourState(CentipedeState.CHASING) && !enemy.IsBehaviourState(CentipedeState.CLINGING) && enemy.clingingToPlayer == null)
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

	public string GetPrimarySkillName(CentipedeAI enemy)
	{
		if (enemy.IsBehaviourState(CentipedeState.HIDING))
		{
			return "Drop";
		}
		return enemy.IsBehaviourState(CentipedeState.CHASING) ? "Attacking (touch a player)" : "Attack mode";
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
