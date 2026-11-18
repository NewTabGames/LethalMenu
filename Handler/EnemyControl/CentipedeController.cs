using HarmonyLib;
using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class CentipedeController : IEnemyController<CentipedeAI>, IController
{
	private static readonly AccessTools.FieldRef<CentipedeAI, bool> TriggeredFall = AccessTools.FieldRefAccess<CentipedeAI, bool>("triggeredFall");

	private bool IsClingingToSomething(CentipedeAI enemy)
	{
		ReflectionUtil<CentipedeAI> reflectionUtil = enemy.Reflect();
		return (object)enemy.clingingToPlayer != null || enemy.inSpecialAnimation || reflectionUtil.GetValue<bool>("clingingToDeadBody") || reflectionUtil.GetValue<bool>("clingingToCeiling") || reflectionUtil.GetValue<bool>("startedCeilingAnimationCoroutine") || reflectionUtil.GetValue<bool>("inDroppingOffPlayerAnim");
	}

	public void Update(CentipedeAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null)
		{
			return;
		}
		enemy.targetNode = null;
		if (enemy.IsBehaviourState(CentipedeState.HIDING))
		{
			TriggeredFall(enemy) = true;
		}
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
		if (enemy.IsOwner && !IsClingingToSomething(enemy))
		{
			// The game's raycast switches to hiding (and syncs it) only when it finds a ceiling. Forcing the state
			// without one made it fly to an old hiding spot, or the middle of the map, and get stuck there.
			enemy.Reflect().Invoke("RaycastToCeiling");
			if (!enemy.IsBehaviourState(CentipedeState.HIDING))
			{
				HUDManager.Instance?.DisplayTip("Lethal Menu", "No ceiling above");
			}
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
