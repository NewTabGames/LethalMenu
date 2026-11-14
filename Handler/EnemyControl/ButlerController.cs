namespace LethalMenu.Handler.EnemyControl;

internal class ButlerController : IEnemyController<ButlerEnemyAI>, IController
{
	public void UsePrimarySkill(ButlerEnemyAI enemy)
	{
		enemy.SyncSearchingMadlyServerRpc(isSearching: true);
	}

	public void UseSecondarySkill(ButlerEnemyAI enemy)
	{
		enemy.SetSweepingAnimClientRpc(sweeping: true);
	}

	public string GetPrimarySkillName(ButlerEnemyAI _)
	{
		return "Search Mad";
	}

	public string GetSecondarySkillName(ButlerEnemyAI _)
	{
		return "Sweep";
	}

	public bool CanUseEntranceDoors(ButlerEnemyAI _)
	{
		return false;
	}

	public float InteractRange(ButlerEnemyAI _)
	{
		return 5f;
	}
}
