using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class BlobController : IEnemyController<BlobAI>, IController
{
	private void SetTamedTimer(BlobAI enemy, float time)
	{
		enemy.Reflect().SetValue("tamedTimer", time);
	}

	private void SetAngeredTimer(BlobAI enemy, float time)
	{
		enemy.Reflect().SetValue("angeredTimer", time);
	}

	// Calming has no network message in the game, so it only works on your machine: it stops the blob hurting your
	// own body, not other players.
	public void OnSecondarySkillHold(BlobAI enemy)
	{
		SetAngeredTimer(enemy, 0f);
		SetTamedTimer(enemy, 2f);
	}

	public void ReleaseSecondarySkill(BlobAI enemy)
	{
		SetTamedTimer(enemy, 0f);
	}

	/// <summary>
	/// Setting the anger timer directly only changed it on your screen. A zero-damage hit is the game's own synced
	/// way to anger it: everyone sees and hears it get agitated.
	/// </summary>
	public void UsePrimarySkill(BlobAI enemy)
	{
		SetAngeredTimer(enemy, 18f);
		enemy.HitEnemyServerRpc(0, -1, playHitSFX: false);
	}

	public float InteractRange(BlobAI _)
	{
		return 3.5f;
	}

	public float SprintMultiplier(BlobAI _)
	{
		return 9.8f;
	}

	public bool CanUseEntranceDoors(BlobAI _)
	{
		return false;
	}

	public string GetPrimarySkillName(BlobAI _)
	{
		return "Get angry";
	}

	public string GetSecondarySkillName(BlobAI _)
	{
		return "(HOLD) Calm down (only protects you)";
	}
}
