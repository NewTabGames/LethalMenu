using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class ForestGiantController : IEnemyController<ForestGiantAI>, IController
{
	private bool IsUsingSecondarySkill { get; set; } = false;

	/// <summary>The chase target last sent to everyone, so it is only sent again when it changes.</summary>
	private GameNetcodeStuff.PlayerControllerB? lastSentTarget;

	public void Update(ForestGiantAI enemy, bool isAIControlled)
	{
		// The AI drives now (F9): a chase held from before must not stay on once you take over again.
		if (isAIControlled)
		{
			IsUsingSecondarySkill = false;
			lastSentTarget = null;
		}
	}

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
		// Setting it only here left everyone else's giant chasing nobody (an error every frame on their side).
		// The game's own chase RPC sets the target and chase mode for every player.
		// (Only the owner may send it; you become the owner a moment after taking control.)
		if (target != lastSentTarget && enemy.IsOwner)
		{
			lastSentTarget = target;
			enemy.Reflect().Invoke("BeginChasingNewPlayerServerRpc", (int)target.playerClientId);
		}
		enemy.SetBehaviourState(GiantState.CHASE);
	}

	public void ReleaseSecondarySkill(ForestGiantAI enemy)
	{
		IsUsingSecondarySkill = false;
		lastSentTarget = null;
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
		lastSentTarget = null;
	}

	public bool SyncAnimationSpeedEnabled(ForestGiantAI _)
	{
		return false;
	}
}
