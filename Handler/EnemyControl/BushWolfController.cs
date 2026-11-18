using GameNetcodeStuff;
using LethalMenu.Util;
using UnityEngine;

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
		// The victim's game only counts the hit if they're closer than attackDistance and nothing blocks the line
		// from the tongue to their head; anyone further away always dodged, even though we said they were in range.
		PlayerControllerB? player = ControlUtil.GetNearestPlayer(enemy, enemy.attackDistance - 0.5f);
		if (player == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No player in tongue range");
			return;
		}
		if (enemy.tongueStartPoint != null && Physics.Linecast(enemy.tongueStartPoint.position, player.gameplayCamera.transform.position - Vector3.up * 0.3f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No clear shot");
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
		// Its own code made random mating calls (which froze its legs for 2 s while you kept moving) and growled and
		// turned toward its last victim by itself. Keep calls and growls on your buttons only.
		if (!isAIControlled && !draggingToDen && !enemy.IsBehaviourState(BushWolfState.ATTACKING))
		{
			enemy.targetPlayer = null;
			enemy.staringAtPlayer = null;
			ReflectionUtil<BushWolfEnemy> reflect = enemy.Reflect();
			reflect.SetValue("timeSpentHiding", 0f);
			// Under 7 s since its last call means no new one; 5 s or more means it doesn't count as just called
			// (which makes it stand out to other monsters).
			if (Time.realtimeSinceStartup - reflect.GetValue<float>("timeSinceCall") > 5.5f)
			{
				reflect.SetValue("timeSinceCall", Time.realtimeSinceStartup - 5.5f);
			}
		}
	}

	public void OnReleaseControl(BushWolfEnemy enemy)
	{
		draggingToDen = false;
	}

	public void UseSecondarySkill(BushWolfEnemy enemy)
	{
		// The growl message only plays on other machines; the game's local growl plays it here too and sends it.
		enemy.Reflect().Invoke("DoGrowlLocalClient");
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
