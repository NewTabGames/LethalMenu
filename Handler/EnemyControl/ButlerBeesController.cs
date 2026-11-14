namespace LethalMenu.Handler.EnemyControl;

internal class ButlerBeesController : IEnemyController<ButlerBeesEnemyAI>, IController
{
	public bool CanUseEntranceDoors(ButlerBeesEnemyAI _)
	{
		return false;
	}

	public float InteractRange(ButlerBeesEnemyAI _)
	{
		return 2.5f;
	}
}
