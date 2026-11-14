using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class ForestGiantController : IEnemyController<ForestGiantAI>, IController
{
	private bool IsUsingSecondarySkill { get; set; } = false;

	public void OnMovement(ForestGiantAI enemy, bool isMoving, bool isSprinting)
	{
		if (!IsUsingSecondarySkill)
		{
			enemy.SetBehaviourState(GiantState.DEFAULT);
		}
	}

	public void OnSecondarySkillHold(ForestGiantAI enemy)
	{
		// The game's chase state reads chasingPlayer every frame and errors if it is empty.
		GameNetcodeStuff.PlayerControllerB target = Cheats.EnemyControl.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 80f, requireLineOfSight: false);
		if (target == null && LethalMenu.localPlayer != null && !LethalMenu.localPlayer.isPlayerDead)
		{
			target = LethalMenu.localPlayer; // alone in the lobby
		}
		if (target == null)
		{
			IsUsingSecondarySkill = false;
			enemy.SetBehaviourState(GiantState.DEFAULT);
			return;
		}
		IsUsingSecondarySkill = true;
		enemy.chasingPlayer = target;
		enemy.SetBehaviourState(GiantState.CHASE);
	}

	public void ReleaseSecondarySkill(ForestGiantAI enemy)
	{
		IsUsingSecondarySkill = false;
		enemy.SetBehaviourState(GiantState.DEFAULT);
	}

	public bool IsAbleToMove(ForestGiantAI enemy)
	{
		return !enemy.Reflect().GetValue<bool>("inEatingPlayerAnimation");
	}

	public string GetSecondarySkillName(ForestGiantAI _)
	{
		return "(HOLD) Chase";
	}

	public bool CanUseEntranceDoors(ForestGiantAI _)
	{
		return false;
	}

	public float InteractRange(ForestGiantAI _)
	{
		return 0f;
	}

	public void OnReleaseControl(ForestGiantAI enemy)
	{
		IsUsingSecondarySkill = false;
	}

	public bool SyncAnimationSpeedEnabled(ForestGiantAI _)
	{
		return false;
	}
}
