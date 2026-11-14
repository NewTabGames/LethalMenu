using GameNetcodeStuff;
using LethalMenu.Util;

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
		// The tongue only kills once the fox has hauled the player back to its den. Like in the game, its own AI
		// does that part: it takes over until the attack is over (eaten, or they got away), then you're back.
		Cheats.EnemyControl.SetAIDriving(true);
		draggingToDen = true;
	}

	/// <summary>Its AI is hauling someone back to the den for you (see <see cref="UsePrimarySkill"/>).</summary>
	private bool draggingToDen;

	public void Update(BushWolfEnemy enemy, bool isAIControlled)
	{
		if (draggingToDen && (!enemy.IsBehaviourState(BushWolfState.ATTACKING) || enemy.isEnemyDead))
		{
			draggingToDen = false;
			Cheats.EnemyControl.SetAIDriving(false);
			return;
		}
		// Its AI gives up dragging once it has dragged for 12 s and is still over 20 m from its den (or after
		// 35 s), so a far den meant letting go halfway. Keep that clock low: it drags them all the way home.
		if (draggingToDen && enemy.Reflect().GetValue<float>("shootTongueTimer") > 10f)
		{
			enemy.Reflect().SetValue("shootTongueTimer", 10f);
		}
	}

	public void OnReleaseControl(BushWolfEnemy enemy)
	{
		draggingToDen = false;
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
		return draggingToDen ? "Dragging them back to the den..." : "Shoot tongue (drags them to the den and eats them)";
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
