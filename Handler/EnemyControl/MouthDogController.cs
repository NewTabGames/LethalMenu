using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class MouthDogController : IEnemyController<MouthDogAI>, IController
{
	private static readonly AccessTools.FieldRef<MouthDogAI, bool> InLunge = AccessTools.FieldRefAccess<MouthDogAI, bool>("inLunge");

	private static readonly AccessTools.FieldRef<MouthDogAI, float> AITimer = AccessTools.FieldRefAccess<MouthDogAI, float>("AITimer");

	private static readonly AccessTools.FieldRef<MouthDogAI, float> LungeCooldown = AccessTools.FieldRefAccess<MouthDogAI, float>("lungeCooldown");

	public void Update(MouthDogAI enemy, bool isAIControlled)
	{
		// In chase mode the game lowers the dog's suspicion every 3 s and drops it out of chase, and sprinting put
		// it straight back, so it howled and called every other dog over again every few seconds. It could also
		// lunge by itself near an old noise. Keep it in chase and leave lunging to you.
		if (isAIControlled || enemy == null || enemy.currentBehaviourStateIndex != (int)DogState.CHASE)
		{
			return;
		}
		enemy.suspicionLevel = Mathf.Max(enemy.suspicionLevel, 11);
		AITimer(enemy) = 3f;
		LungeCooldown(enemy) = 1f;
	}

	public void OnMovement(MouthDogAI enemy, bool isMoving, bool isSprinting)
	{
		// Mid-lunge: the game ends the lunge itself. Walking used to overwrite it right away.
		if (enemy.currentBehaviourStateIndex == (int)DogState.LUNGE)
		{
			return;
		}
		if (!isSprinting)
		{
			if (isMoving)
			{
				enemy.SetBehaviourState(DogState.ROAMING);
			}
		}
		else
		{
			enemy.SetBehaviourState(DogState.CHASE);
		}
	}

	// Set it up like the game's own lunge (EnterLunge), so its timer ends it again after about 2 s. Just switching
	// to the lunge state didn't move the dog, and after the first lunge it never ended at all.
	public void UseSecondarySkill(MouthDogAI enemy)
	{
		if (enemy.currentBehaviourStateIndex == (int)DogState.LUNGE)
		{
			return;
		}
		enemy.endingLunge = false;
		InLunge(enemy) = true;
		if (enemy.agent != null)
		{
			enemy.agent.speed = 13f;
		}
		enemy.SwitchToBehaviourState((int)DogState.LUNGE);
		Cheats.EnemyControl.Dash(enemy.transform.forward, 13f, 0.7f);
	}

	public string GetSecondarySkillName(MouthDogAI _)
	{
		return "Lunge";
	}

	public float InteractRange(MouthDogAI _)
	{
		return 2.5f;
	}

	public bool CanUseEntranceDoors(MouthDogAI _)
	{
		return false;
	}
}

[HarmonyPatch]
internal static class MouthDogAIPatches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(MouthDogAI), nameof(MouthDogAI.DetectNoise))]
	public static bool DetectNoise(MouthDogAI __instance)
	{
		return !Cheats.EnemyControl.IsDrivenByYou(__instance);
	}
}
