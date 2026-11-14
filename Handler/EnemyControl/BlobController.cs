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

	public void OnSecondarySkillHold(BlobAI enemy)
	{
		SetAngeredTimer(enemy, 0f);
		SetTamedTimer(enemy, 2f);
	}

	public void ReleaseSecondarySkill(BlobAI enemy)
	{
		SetTamedTimer(enemy, 0f);
	}

	public void UsePrimarySkill(BlobAI enemy)
	{
		SetAngeredTimer(enemy, 18f);
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
		return "(HOLD) Calm down";
	}
}
