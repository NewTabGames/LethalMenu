using System.Collections;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class SandWormController : IEnemyController<SandWormAI>, IController
{
	public static bool EmergeCooldown = false;

	private static int Seconds = 20;

	public void Update(SandWormAI enemy, bool isAIControlled)
	{
		if (!((enemy == null) | isAIControlled))
		{
			enemy.chaseTimer = 2f;
		}
	}

	public void UsePrimarySkill(SandWormAI enemy)
	{
		if (!(enemy == null) && !enemy.inEmergingState && !EmergeCooldown)
		{
			enemy.StartEmergeAnimation();
			LethalMenu.Instance.StartCoroutine(StartEmergeCoolDown());
		}
	}

	public string GetPrimarySkillName(SandWormAI _)
	{
		return EmergeCooldown ? $"Emerge Cooldown ({Seconds} seconds)" : "Emerge";
	}

	public bool CanUseEntranceDoors(SandWormAI _)
	{
		return false;
	}

	public float InteractRange(SandWormAI _)
	{
		return 0f;
	}

	public bool SyncAnimationSpeedEnabled(SandWormAI _)
	{
		return false;
	}

	private IEnumerator StartEmergeCoolDown()
	{
		EmergeCooldown = true;
		for (Seconds = 20; Seconds > 0; Seconds--)
		{
			yield return new WaitForSeconds(1f);
		}
		EmergeCooldown = false;
	}
}
