namespace LethalMenu.Handler.EnemyControl;

internal class MaskedPlayerController : IEnemyController<MaskedPlayerEnemy>, IController
{
	public void UsePrimarySkill(MaskedPlayerEnemy enemy)
	{
		enemy.SetHandsOutServerRpc(!enemy.creatureAnimator.GetBool("HandsOut"));
	}

	public void UseSecondarySkill(MaskedPlayerEnemy enemy)
	{
		enemy.SetCrouchingServerRpc(!enemy.creatureAnimator.GetBool("Crouching"));
	}

	public float InteractRange(MaskedPlayerEnemy _)
	{
		return 1f;
	}

	public bool SyncAnimationSpeedEnabled(MaskedPlayerEnemy _)
	{
		return false;
	}

	public bool CanUseEntranceDoors(MaskedPlayerEnemy _)
	{
		return true;
	}

	public string GetPrimarySkillName(MaskedPlayerEnemy enemy)
	{
		return enemy.creatureAnimator.GetBool("HandsOut") ? "Lower hands" : "Raise hands";
	}

	public string GetSecondarySkillName(MaskedPlayerEnemy enemy)
	{
		return enemy.creatureAnimator.GetBool("Crouching") ? "Stand up" : "Crouch";
	}
}
