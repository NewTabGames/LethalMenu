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

	private bool wasJumping;

	/// <summary>
	/// Each hop lasts only about 0.2 s. Your movement speeds up gradually, so in that time it barely got going
	/// and the Barber hardly moved. At the start of each hop, throw it the same distance the game's hop covers,
	/// in the direction you're steering.
	/// </summary>
	public void Update(ClaySurgeonAI enemy, bool isAIControlled)
	{
		bool jumping = enemy.isJumping;
		if (jumping && !wasJumping && !isAIControlled)
		{
			UnityEngine.Vector3 direction = Cheats.EnemyControl.SteerDirection;
			if (direction != UnityEngine.Vector3.zero)
			{
				float hopTime = UnityEngine.Mathf.Max(enemy.jumpTime, 0.05f);
				Cheats.EnemyControl.Dash(direction, HopSpeed(enemy, hopTime), hopTime);
			}
		}
		wasJumping = jumping;
	}

	/// <summary>
	/// The game sets its speed to jumpSpeed for the hop, and its walker starts from a standstill and speeds up at
	/// its acceleration: the average speed over one hop.
	/// </summary>
	internal static float HopSpeed(ClaySurgeonAI enemy, float hopTime)
	{
		float v = enemy.jumpSpeed;
		float accel = enemy.agent != null ? enemy.agent.acceleration : 0f;
		if (v <= 0.5f)
		{
			return 3.5f;
		}
		float distance = accel <= 0.01f ? v * hopTime : (accel * hopTime < v ? 0.5f * accel * hopTime * hopTime : v * hopTime - v * v / (2f * accel));
		return UnityEngine.Mathf.Clamp(distance / hopTime, 1.5f, v);
	}

	public void OnReleaseControl(ClaySurgeonAI enemy)
	{
		wasJumping = false;
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
