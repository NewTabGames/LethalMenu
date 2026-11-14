using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class DressGirlController : IEnemyController<DressGirlAI>, IController
{
	public void UsePrimarySkill(DressGirlAI enemy)
	{
		if (enemy == null)
		{
			return;
		}
		PlayerControllerB closestPlayer = GetClosestPlayer(enemy);
		if (closestPlayer != null)
		{
			enemy.hauntingPlayer = closestPlayer;
			if (enemy.hauntingPlayer != null && enemy.currentBehaviourStateIndex != 1)
			{
				((EnemyAI)enemy).EnableEnemyMesh(true, true);
				enemy.SwitchToBehaviourStateOnLocalClient(0);
				enemy.Reflect().Invoke("BeginChasing");
			}
			else if (enemy.hauntingPlayer != null && enemy.currentBehaviourStateIndex == 1)
			{
				enemy.Reflect().Invoke("StopChasing");
			}
		}
	}

	public string GetPrimarySkillName(DressGirlAI enemy)
	{
		PlayerControllerB closestPlayer = GetClosestPlayer(enemy);
		string result;
		if (closestPlayer == null)
		{
			result = "";
		}
		else
		{
			result = ((enemy.currentBehaviourStateIndex == 1) ? "Stop Chase" : "Begin Chase");
		}
		return result;
	}

	public void OnReleaseControl(DressGirlAI enemy)
	{
		if (!(enemy == null))
		{
			if (enemy.currentBehaviourStateIndex == 1)
			{
				enemy.Reflect().Invoke("StopChasing");
			}
			enemy.Handle().Teleport(null, new Vector3(-1000f, -1000f));
			((EnemyAI)enemy).EnableEnemyMesh(false, true);
			enemy.hauntingPlayer = null;
		}
	}

	public void OnDeath(DressGirlAI enemy)
	{
		if (enemy.currentBehaviourStateIndex == 1)
		{
			enemy.Reflect().Invoke("StopChasing");
		}
		enemy.Handle().Teleport(null, new Vector3(-1000f, -1000f));
		((EnemyAI)enemy).EnableEnemyMesh(false, true);
		enemy.hauntingPlayer = null;
	}

	public bool CanUseEntranceDoors(DressGirlAI _)
	{
		return true;
	}

	public float InteractRange(DressGirlAI _)
	{
		return 5f;
	}

	public static PlayerControllerB GetClosestPlayer(EnemyAI e)
	{
		// The player picked with T / the website wins while this monster is the one being controlled.
		if (Cheats.EnemyControl.ControlledEnemy == e && Cheats.EnemyControl.ValidChosenTarget != null)
		{
			return Cheats.EnemyControl.ValidChosenTarget;
		}
		return (from p in LethalMenu.players
			where p != null && !p.isPlayerDead
			orderby Vector3.Distance(e.transform.position, p.transform.position)
			select p).FirstOrDefault();
	}
}
