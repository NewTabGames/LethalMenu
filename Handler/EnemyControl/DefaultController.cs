namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Fallback for enemies without a dedicated controller (future game updates, modded enemies).
/// Movement, doors and the AI toggle still work; there are no special skills.
/// </summary>
internal class DefaultController : IEnemyController<EnemyAI>, IController
{
	public string GetPrimarySkillName(EnemyAI _)
	{
		return "(No skills for this enemy)";
	}
}
