using GameNetcodeStuff;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal enum PumaState
{
	ROAMING,
	STALKING,
	ATTACKING
}

internal class PumaController : IEnemyController<PumaAI>, IController
{
	public void Update(PumaAI enemy, bool isAIControlled)
	{
		if (!isAIControlled)
		{
			// Keep the puma on the ground while it is being driven.
			enemy.clingingToTree = false;
		}
	}

	public void UsePrimarySkill(PumaAI enemy)
	{
		PlayerControllerB? player = ControlUtil.GetNearestPlayer(enemy, 25f);
		if (player == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No player in pounce range");
			return;
		}
		enemy.targetPlayer = player;
		enemy.SetBehaviourState(PumaState.ATTACKING);
	}

	public void OnSecondarySkillHold(PumaAI enemy)
	{
		// The puma only damages on contact shortly after a scratch animation event; hold that window open.
		enemy.Reflect().SetValue("timeAtLastScratch", Time.realtimeSinceStartup);
	}

	public void OnMovement(PumaAI enemy, bool isMoving, bool isSprinting)
	{
		if (!enemy.IsBehaviourState(PumaState.ATTACKING))
		{
			enemy.SetBehaviourState(isMoving ? PumaState.STALKING : PumaState.ROAMING);
		}
	}

	public bool IsAbleToMove(PumaAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(PumaAI _)
	{
		return "Pounce on nearest player";
	}

	public string GetSecondarySkillName(PumaAI _)
	{
		return "(HOLD) Scratch on contact";
	}

	public bool CanUseEntranceDoors(PumaAI _)
	{
		return false;
	}

	public float SprintMultiplier(PumaAI _)
	{
		return 3.5f;
	}
}
