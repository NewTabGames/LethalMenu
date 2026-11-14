namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Barber. It only ever moves in hops on the beat of its music (all Barbers hop together), so driven it hops too:
/// you move only during each hop, at its hop speed. Walk into a player to cut them, like in the game.
/// (Before, it slid along smoothly while its legs hopped on their own beat.)
/// </summary>
internal class ClaySurgeonController : IEnemyController<ClaySurgeonAI>, IController
{
	public void OnTakeControl(ClaySurgeonAI enemy)
	{
		enemy.SyncMasterClaySurgeonClientRpc();
	}

	public bool IsAbleToMove(ClaySurgeonAI enemy)
	{
		return enemy.isJumping;
	}

	public string GetPrimarySkillName(ClaySurgeonAI _)
	{
		return "Hops on the beat; walk into players to cut them";
	}

	public string GetSecondarySkillName(ClaySurgeonAI _)
	{
		return "";
	}

	public bool CanUseEntranceDoors(ClaySurgeonAI _)
	{
		return false;
	}

	public float InteractRange(ClaySurgeonAI _)
	{
		return 2.5f;
	}
}
