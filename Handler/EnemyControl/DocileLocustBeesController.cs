namespace LethalMenu.Handler.EnemyControl;

internal class DocileLocustBeesController : IEnemyController<DocileLocustBeesAI>, IController
{
	public bool CanUseEntranceDoors(DocileLocustBeesAI _)
	{
		return false;
	}

	public float InteractRange(DocileLocustBeesAI _)
	{
		return 2.5f;
	}
}
