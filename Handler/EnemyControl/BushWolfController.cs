using GameNetcodeStuff;

namespace LethalMenu.Handler.EnemyControl;

internal enum BushWolfState
{
	HIDING,
	STALKING,
	ATTACKING
}

internal class BushWolfController : IEnemyController<BushWolfEnemy>, IController
{
	public void UsePrimarySkill(BushWolfEnemy enemy)
	{
		if (enemy.IsBehaviourState(BushWolfState.ATTACKING))
		{
			return;
		}
		PlayerControllerB? player = ControlUtil.GetNearestPlayer(enemy, enemy.attackDistance + 10f);
		if (player == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No player in tongue range");
			return;
		}
		enemy.targetPlayer = player;
		enemy.SwitchToBehaviourStateOnLocalClient((int)BushWolfState.ATTACKING);
		enemy.SyncTargetPlayerAndAttackServerRpc((int)player.playerClientId);
	}

	public void UseSecondarySkill(BushWolfEnemy enemy)
	{
		enemy.DoGrowlServerRpc();
	}

	public void OnMovement(BushWolfEnemy enemy, bool isMoving, bool isSprinting)
	{
		if (!enemy.IsBehaviourState(BushWolfState.ATTACKING))
		{
			enemy.SetBehaviourState(isMoving ? BushWolfState.STALKING : BushWolfState.HIDING);
		}
	}

	public string GetPrimarySkillName(BushWolfEnemy _)
	{
		return "Shoot tongue at nearest player";
	}

	public string GetSecondarySkillName(BushWolfEnemy _)
	{
		return "Growl";
	}

	public bool CanUseEntranceDoors(BushWolfEnemy _)
	{
		return false;
	}
}
