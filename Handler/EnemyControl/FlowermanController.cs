using HarmonyLib;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class FlowermanController : IEnemyController<FlowermanAI>, IController
{
	internal static bool HoldStand;

	public void OnTakeControl(FlowermanAI enemy)
	{
		HoldStand = false;
	}

	public void OnReleaseControl(FlowermanAI enemy)
	{
		HoldStand = false;
	}

	public void Update(FlowermanAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null)
		{
			HoldStand = false;
			return;
		}
		enemy.evadeStealthTimer = 0f;
		if (!enemy.IsBehaviourState(FlowermanState.ANGER))
		{
			enemy.angerMeter = 0f;
		}
		// Keep its walker switched on like every other driven monster (your steering moves it, not the walker).
		// Switched off, letting go or pressing F9 couldn't put it back on the floor or path it, so it stood frozen.
		if (!enemy.inSpecialAnimation && enemy.agent != null && !enemy.agent.enabled)
		{
			enemy.agent.enabled = true;
		}
		if (enemy.IsBehaviourState(FlowermanState.STAND) && !HoldStand && !enemy.carryingPlayerBody)
		{
			enemy.SetBehaviourState(FlowermanState.SCOUTING);
		}
	}

	public void UsePrimarySkill(FlowermanAI enemy)
	{
		if (!enemy.IsOwner)
		{
			return;
		}
		if (enemy.carryingPlayerBody)
		{
			enemy.DropPlayerBodyServerRpc();
			return;
		}
		enemy.EnterAngerModeServerRpc(12f);
		enemy.SetBehaviourState(FlowermanState.ANGER);
	}

	public void UseSecondarySkill(FlowermanAI enemy)
	{
		HoldStand = true;
		enemy.SetBehaviourState(FlowermanState.STAND);
	}

	public void OnSecondarySkillHold(FlowermanAI enemy)
	{
		HoldStand = true;
		if (!enemy.IsBehaviourState(FlowermanState.STAND))
		{
			enemy.SetBehaviourState(FlowermanState.STAND);
		}
	}

	public void ReleaseSecondarySkill(FlowermanAI enemy)
	{
		HoldStand = false;
		if (!enemy.carryingPlayerBody)
		{
			enemy.SetBehaviourState(FlowermanState.SCOUTING);
		}
	}

	public bool IsAbleToMove(FlowermanAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(FlowermanAI enemy)
	{
		return enemy.carryingPlayerBody ? "Drop body" : "Get angry";
	}

	public string GetSecondarySkillName(FlowermanAI _)
	{
		return "(HOLD) Stand up";
	}

	public float InteractRange(FlowermanAI _)
	{
		return 1.5f;
	}

	public bool SyncAnimationSpeedEnabled(FlowermanAI _)
	{
		return false;
	}

	public bool CanUseEntranceDoors(FlowermanAI _)
	{
		return false;
	}
}

[HarmonyPatch]
internal static class FlowermanAIPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(FlowermanAI), "LookAtPlayerOfInterest")]
	public static bool LookAtPlayerOfInterest(FlowermanAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.LookAtFlowermanTrigger))]
	public static bool LookAtFlowermanTrigger(FlowermanAI __instance)
	{
		return !Control.IsDrivenByYou(__instance);
	}
}
