using System;
using System.Reflection;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal enum StingrayState
{
	ROAMING,
	CLOAKED
}

internal class StingrayController : IEnemyController<StingrayAI>, IController
{
	private static readonly MethodInfo? CreateSlimeLocal = AccessTools.Method(typeof(StingrayAI), "CreateSlime", new Type[1] { typeof(Vector3) });

	private float slimeCooldown;

	public void Update(StingrayAI enemy, bool isAIControlled)
	{
		slimeCooldown -= Time.deltaTime;
		if (!isAIControlled)
		{
			// Stop the AI cloaking on its own when it thinks it has reached a hiding spot.
			StingrayHidingSpot? hidingSpot = enemy.Reflect().GetValue<StingrayHidingSpot>("hidingSpot");
			if (hidingSpot != null)
			{
				hidingSpot.gotHidingSpot = false;
			}
		}
	}

	public void UsePrimarySkill(StingrayAI enemy)
	{
		// Leaving the cloaked state makes the game perform its lunge + spit.
		enemy.SetBehaviourState(enemy.IsBehaviourState(StingrayState.CLOAKED) ? StingrayState.ROAMING : StingrayState.CLOAKED);
	}

	public void OnSecondarySkillHold(StingrayAI enemy)
	{
		if (slimeCooldown > 0f || enemy.IsBehaviourState(StingrayState.CLOAKED))
		{
			return;
		}
		slimeCooldown = 0.35f;
		Vector3 position = enemy.transform.position;
		// CreateSlime is overloaded (Vector3 / Vector3[]), so resolve the exact signature. The RPC only reaches other clients.
		CreateSlimeLocal?.Invoke(enemy, new object[1] { position });
		enemy.CreateSlimeRpc(position);
	}

	public bool IsAbleToMove(StingrayAI enemy)
	{
		return !enemy.IsBehaviourState(StingrayState.CLOAKED);
	}

	public string GetPrimarySkillName(StingrayAI enemy)
	{
		return enemy.IsBehaviourState(StingrayState.CLOAKED) ? "Lunge out of cloak" : "Cloak";
	}

	public string GetSecondarySkillName(StingrayAI _)
	{
		return "(HOLD) Leave slime trail";
	}

	public bool CanUseEntranceDoors(StingrayAI _)
	{
		return false;
	}
}
