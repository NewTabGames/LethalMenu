namespace LethalMenu.Handler.EnemyControl;

internal enum CadaverBloomState
{
	SEARCHING,
	CHASING,
	BURSTING
}

internal class CadaverBloomController : IEnemyController<CadaverBloomAI>, IController
{
	private bool chomping;

	public void OnTakeControl(CadaverBloomAI enemy)
	{
		ForgetTarget(enemy);
	}

	public void Update(CadaverBloomAI enemy, bool isAIControlled)
	{
		if (isAIControlled)
		{
			// Right click held when F9 was pressed: the release never reaches us, so stop here.
			StopChomping(enemy);
			return;
		}
		ForgetTarget(enemy);
	}

	/// <summary>
	/// Taken mid-chase, its chase code kept the old target (only its paused AI updates it): it turned toward that
	/// player every frame, fighting your mouse, and chomped and roared by itself. Under F9 its AI picks a new one.
	/// </summary>
	private static void ForgetTarget(CadaverBloomAI enemy)
	{
		if (enemy.targetPlayer != null)
		{
			enemy.targetPlayer = null;
		}
	}

	public void OnMovement(CadaverBloomAI enemy, bool isMoving, bool isSprinting)
	{
		if (!enemy.IsBehaviourState(CadaverBloomState.BURSTING))
		{
			enemy.SetBehaviourState(isSprinting ? CadaverBloomState.CHASING : CadaverBloomState.SEARCHING);
		}
	}

	public void OnSecondarySkillHold(CadaverBloomAI enemy)
	{
		if (!chomping)
		{
			chomping = true;
			SetChomping(enemy, true);
		}
	}

	public void ReleaseSecondarySkill(CadaverBloomAI enemy)
	{
		StopChomping(enemy);
	}

	public void OnReleaseControl(CadaverBloomAI enemy)
	{
		StopChomping(enemy);
	}

	private void StopChomping(CadaverBloomAI enemy)
	{
		if (chomping)
		{
			chomping = false;
			SetChomping(enemy, false);
		}
	}

	/// <summary>
	/// The chomp message only reaches other players, so you never saw or heard your own chomp. Play it here too.
	/// (chestPlantVisible is left alone: here, its own code would see it while walking and hide the chest for everyone.)
	/// </summary>
	private static void SetChomping(CadaverBloomAI enemy, bool on)
	{
		if (enemy.creatureAnimator != null)
		{
			enemy.creatureAnimator.SetBool("ChestVisible", on);
			enemy.creatureAnimator.SetBool("ChestBite", on);
		}
		if (enemy.burstSource != null)
		{
			if (on && !enemy.burstSource.isPlaying)
			{
				enemy.burstSource.Play();
			}
			else if (!on && enemy.burstSource.isPlaying)
			{
				enemy.burstSource.Stop();
			}
		}
		enemy.SyncChompingAnimRpc(on);
	}

	public bool IsAbleToMove(CadaverBloomAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(CadaverBloomAI _)
	{
		return "Walk into players to bite";
	}

	public string GetSecondarySkillName(CadaverBloomAI _)
	{
		return "(HOLD) Chomp";
	}
}
