namespace LethalMenu.Handler.EnemyControl;

internal class RadMechController : IEnemyController<RadMechAI>, IController
{
	public void OnTakeControl(RadMechAI enemy)
	{
	}

	public void OnReleaseControl(RadMechAI enemy)
	{
	}

	public string GetPrimarySkillName(RadMechAI _)
	{
		return "Fire";
	}

	public void UsePrimarySkill(RadMechAI enemy)
	{
	}

	public string GetSecondarySkillName(RadMechAI _)
	{
		return "";
	}

	public bool CanUseEntranceDoors(RadMechAI _)
	{
		return false;
	}

	public float InteractRange(RadMechAI _)
	{
		return 10f;
	}
}
