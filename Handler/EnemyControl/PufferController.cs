using GameNetcodeStuff;
using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

internal class PufferController : IEnemyController<PufferAI>, IController
{
	private static bool playerIsInLOS;

	private static PlayerControllerB player;

	public void UsePrimarySkill(PufferAI enemy)
	{
		enemy.SetBehaviourState(PufferState.HOSTILE);
		player = DressGirlController.GetClosestPlayer(enemy);
		if (!(player == null))
		{
			playerIsInLOS = player;
			enemy.Reflect().SetValue("closestSeenPlayer", player);
			enemy.Reflect().SetValue("playerIsInLOS", playerIsInLOS);
			enemy.StompServerRpc();
			player = null;
			playerIsInLOS = false;
		}
	}

	public void UseSecondarySkill(PufferAI enemy)
	{
		enemy.SetBehaviourState(PufferState.HOSTILE);
		player = DressGirlController.GetClosestPlayer(enemy);
		if (!(player == null))
		{
			playerIsInLOS = player;
			enemy.Reflect().SetValue("closestSeenPlayer", player);
			enemy.Reflect().SetValue("playerIsInLOS", playerIsInLOS);
			enemy.ShakeTailServerRpc();
			player = null;
			playerIsInLOS = false;
		}
	}

	public string GetPrimarySkillName(PufferAI _)
	{
		return "Stomp";
	}

	public string GetSecondarySkillName(PufferAI _)
	{
		return "Smoke";
	}

	public float InteractRange(PufferAI _)
	{
		return 2.5f;
	}

	public bool CanUseEntranceDoors(PufferAI _)
	{
		return false;
	}
}
