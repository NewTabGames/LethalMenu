using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

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
		if (!(enemy == null) && !enemy.inEmergingState && !enemy.emerged && !EmergeCooldown)
		{
			if (!enemy.IsServer && !enemy.IsOwner)
			{
				HUDManager.Instance?.DisplayTip("Lethal Menu", "Taking control of the worm, try again");
				return;
			}
			enemy.StartEmergeAnimation();
			// It doesn't come up when there's no clear arc over open ground; don't make you wait for nothing then.
			if (!enemy.inEmergingState)
			{
				HUDManager.Instance?.DisplayTip("Lethal Menu", "No room to emerge here");
				return;
			}
			LethalMenu.Instance.StartCoroutine(StartEmergeCoolDown());
		}
	}

	public void OnReleaseControl(SandWormAI enemy)
	{
		// A non-host driver keeps the worm afterwards, and only the host can make it emerge: it never came up
		// again for the rest of the round. Give it back to the host once it is back under the ground.
		if (enemy != null && !enemy.IsServer && enemy.IsOwner)
		{
			LethalMenu.Instance.StartCoroutine(GiveBackToHost(enemy));
		}
	}

	private static IEnumerator GiveBackToHost(SandWormAI enemy)
	{
		// Mid-leap only this machine knows where it lands; handing it over then dropped it at a wrong spot.
		float giveUpAt = Time.time + 20f;
		while (enemy != null && (enemy.inSpecialAnimation || enemy.inEmergingState || enemy.emerged) && Time.time < giveUpAt)
		{
			yield return null;
		}
		if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned || !enemy.IsOwner || Cheats.EnemyControl.ControlledEnemy == enemy)
		{
			yield break;
		}
		enemy.SyncPositionToClients();
		yield return new WaitForSeconds(1.5f);
		if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned || !enemy.IsOwner || Cheats.EnemyControl.ControlledEnemy == enemy || StartOfRound.Instance == null)
		{
			yield break;
		}
		enemy.ChangeEnemyOwnerServerRpc(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
	}

	// While it comes up, leaps and dives, the game moves and turns it (its arc is aimed over open ground); your
	// steering pulled it off that arc and, after it landed, back to where it came up.
	public bool IsAbleToMove(SandWormAI enemy)
	{
		return !enemy.inSpecialAnimation && !enemy.inEmergingState && !enemy.emerged;
	}

	public bool IsAbleToRotate(SandWormAI enemy)
	{
		return IsAbleToMove(enemy);
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

/// <summary>
/// The game sets up the worm's leap only on the host, then asks for it with a call only the worm's owner may make.
/// While a non-host player drives it, they own it, so the leap was never set up and nothing happened. On that
/// owner, let the game's own set-up run (only its "host only" check is changed, nothing else).
/// </summary>
[HarmonyPatch]
internal static class SandWormAIPatches
{
	private static readonly MethodInfo IsServerGetter = AccessTools.PropertyGetter(typeof(NetworkBehaviour), nameof(NetworkBehaviour.IsServer));

	/// <summary>Replaces the game's "only the host" check: the host, or the player driving the worm (its owner).</summary>
	public static bool HostOrDriver(NetworkBehaviour behaviour)
	{
		return behaviour.IsServer || (behaviour is SandWormAI worm && worm.IsOwner && Control.IsDrivenByYou(worm));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SandWormAI), nameof(SandWormAI.StartEmergeAnimation))]
	public static void StartEmergeAnimationPrefix(SandWormAI __instance)
	{
		if (!__instance.IsServer && __instance.IsOwner && Control.IsDrivenByYou(__instance))
		{
			// The leap starts from where the owner says it is; on our machine that is where it is right now.
			__instance.serverPosition = __instance.transform.position;
		}
	}

	[HarmonyTranspiler]
	[HarmonyPatch(typeof(SandWormAI), nameof(SandWormAI.StartEmergeAnimation))]
	public static IEnumerable<CodeInstruction> StartEmergeAnimationTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.Calls(IsServerGetter))
			{
				// Same spot and stack (the worm), so any jump into this instruction still lands here.
				instruction.opcode = OpCodes.Call;
				instruction.operand = AccessTools.Method(typeof(SandWormAIPatches), nameof(HostOrDriver));
			}
			yield return instruction;
		}
	}
}
