using GameNetcodeStuff;
using HarmonyLib;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class PufferController : IEnemyController<PufferAI>, IController
{
	private static readonly AccessTools.FieldRef<PufferAI, PlayerControllerB> ClosestSeenPlayer = AccessTools.FieldRefAccess<PufferAI, PlayerControllerB>("closestSeenPlayer");

	private static readonly AccessTools.FieldRef<PufferAI, bool> PlayerIsInLOS = AccessTools.FieldRefAccess<PufferAI, bool>("playerIsInLOS");

	private static readonly AccessTools.FieldRef<PufferAI, float> TimeSinceLookingAtNoise = AccessTools.FieldRefAccess<PufferAI, float>("timeSinceLookingAtNoise");

	public void Update(PufferAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null)
		{
			return;
		}
		ClosestSeenPlayer(enemy) = null;
		PlayerIsInLOS(enemy) = false;
		TimeSinceLookingAtNoise(enemy) = 10f;
	}

	public void UsePrimarySkill(PufferAI enemy)
	{
		if (!enemy.IsOwner)
		{
			return;
		}
		enemy.SetBehaviourState(PufferState.HOSTILE);
		enemy.StompServerRpc();
	}

	public void UseSecondarySkill(PufferAI enemy)
	{
		if (!enemy.IsOwner)
		{
			return;
		}
		Control.EnsureMapPropsContainer();
		enemy.SetBehaviourState(PufferState.HOSTILE);
		enemy.ShakeTailServerRpc();
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
