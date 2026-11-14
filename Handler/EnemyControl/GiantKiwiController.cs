using GameNetcodeStuff;
using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal enum GiantKiwiState
{
	IDLE,
	PATROLLING,
	ATTACKING
}

internal class GiantKiwiController : IEnemyController<GiantKiwiAI>, IController
{
	private bool attacking;

	public void UsePrimarySkill(GiantKiwiAI enemy)
	{
		enemy.Screech(enraged: true, sync: true);
	}

	public void OnSecondarySkillHold(GiantKiwiAI enemy)
	{
		if (attacking)
		{
			return;
		}
		PlayerControllerB? player = ControlUtil.GetNearestPlayer(enemy, 40f);
		if (player == null)
		{
			return;
		}
		attacking = true;
		enemy.targetPlayer = player;
		enemy.Reflect().SetValue("watchingThreat", player);
		enemy.SyncWatchingThreatServerRpc(player.NetworkObject, (int)LethalMenu.localPlayer.playerClientId);
		enemy.SetBehaviourState(GiantKiwiState.ATTACKING);
	}

	public void ReleaseSecondarySkill(GiantKiwiAI enemy)
	{
		StopAttacking(enemy);
	}

	public void OnReleaseControl(GiantKiwiAI enemy)
	{
		StopAttacking(enemy);
	}

	private void StopAttacking(GiantKiwiAI enemy)
	{
		if (attacking)
		{
			attacking = false;
			enemy.SetBehaviourState(GiantKiwiState.PATROLLING);
		}
	}

	public bool IsAbleToMove(GiantKiwiAI enemy)
	{
		return !enemy.inKillAnimation;
	}

	public string GetPrimarySkillName(GiantKiwiAI _)
	{
		return "Screech";
	}

	public string GetSecondarySkillName(GiantKiwiAI _)
	{
		return "(HOLD) Peck nearest player";
	}

	public bool CanUseEntranceDoors(GiantKiwiAI _)
	{
		return false;
	}

	public float InteractRange(GiantKiwiAI _)
	{
		return 0f;
	}
}
