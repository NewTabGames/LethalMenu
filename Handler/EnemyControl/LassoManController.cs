namespace LethalMenu.Handler.EnemyControl;

internal class LassoManController : IEnemyController<LassoManAI>, IController
{
	public void OnTakeControl(LassoManAI _)
	{
		// Its touch only hurts after a timer the game never starts, so it can't hurt anyone, AI or not.
		HUDManager.Instance?.DisplayTip("Lethal Menu", "The Lasso Man can't hurt anyone, even in the normal game");
	}

	private static bool HasScreech(LassoManAI enemy)
	{
		return enemy.creatureVoice != null && enemy.enemyType != null && enemy.enemyType.audioClips != null && enemy.enemyType.audioClips.Length > 0;
	}

	// The game's own screech RPC is empty (no sound, no noise), so the skill did nothing. Play one of its sounds
	// here instead; only you hear it.
	public void UsePrimarySkill(LassoManAI enemy)
	{
		if (!HasScreech(enemy))
		{
			return;
		}
		RoundManager.PlayRandomClip(enemy.creatureVoice, enemy.enemyType.audioClips);
		RoundManager.Instance.PlayAudibleNoise(enemy.transform.position, 15f, 0.8f);
	}

	public bool SyncAnimationSpeedEnabled(LassoManAI enemy)
	{
		return false;
	}

	public bool CanUseEntranceDoors(LassoManAI _)
	{
		return true;
	}

	public string? GetPrimarySkillName(LassoManAI enemy)
	{
		return HasScreech(enemy) ? "Screech (only you hear it)" : null;
	}
}
