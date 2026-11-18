using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal enum SpringManState
{
	ROAMING,
	CHASING
}

/// <summary>
/// CoilHead. Like in the game it freezes while a player looks at it, and kills whoever it runs into when it can
/// move. Hold right click to move even while someone is looking.
/// </summary>
internal class SpringManController : IEnemyController<SpringManAI>, IController
{
	/// <summary>True while right click is held (read by <see cref="SpringManAIPatches"/>).</summary>
	internal static bool HoldGo;

	/// <summary>When a non-host driver next asks the host to reset the CoilHead's tiredness.</summary>
	private float nextTireReset;

	private float nextWakeTry;

	private static bool Stopped(SpringManAI enemy)
	{
		return enemy.Reflect().GetValue<bool>("stoppingMovement");
	}

	public void OnTakeControl(SpringManAI enemy)
	{
		HoldGo = false;
		nextTireReset = 0f;
		nextWakeTry = 0f;
		// After 9 s of moving it rests for 20 s, and only its AI counts that rest down. Its AI is paused while you
		// drive it, so a rest that started under you never ended: it stood frozen with nobody looking, for good.
		// Wake it if it is resting; while you drive it, it doesn't tire (see the patch below).
		enemy.timeSpentMoving = 0f;
		if (enemy.onCooldownPhase > 0f || enemy.Reflect().GetValue<bool>("setOnCooldown"))
		{
			enemy.SetCoilheadOnCooldownServerRpc(false);
		}
		// Its roaming mode plays the walk all the time (it ran in place) and can't hurt anyone; its chase mode
		// has the stop-when-seen rule and the kill on touch.
		if (!enemy.IsBehaviourState(SpringManState.CHASING))
		{
			enemy.SetBehaviourState(SpringManState.CHASING);
		}
	}

	public void OnReleaseControl(SpringManAI enemy)
	{
		HoldGo = false;
		// Hand it back ready to go: it re-checks right away whether anyone is looking, and starts a fresh 9 s
		// before its next rest.
		enemy.timeSpentMoving = 0f;
		enemy.Reflect().SetValue("stopAndGoMinimumInterval", 0f);
	}

	public void Update(SpringManAI enemy, bool isAIControlled)
	{
		if (isAIControlled)
		{
			HoldGo = false;
			return;
		}
		// The 9 s tiredness is counted on the host, not on the driver. With a non-host driver the host kept counting,
		// put it to rest after about 9 s of moving, and nothing ever woke it. Ask the host to reset it now and then
		// (any player may), and at once if it has gone to rest. Each reset blocks kills for a split second, so not
		// more often than needed.
		if (!enemy.IsServer && enemy.IsSpawned)
		{
			bool resting = enemy.onCooldownPhase > 0f || enemy.Reflect().GetValue<bool>("setOnCooldown");
			if ((resting && Time.time >= nextWakeTry) || Time.time >= nextTireReset)
			{
				nextWakeTry = Time.time + 0.5f;
				nextTireReset = Time.time + 7f;
				enemy.SetCoilheadOnCooldownServerRpc(false);
			}
		}
	}

	public void UseSecondarySkill(SpringManAI enemy)
	{
		HoldGo = true;
		if (Stopped(enemy) && enemy.IsOwner)
		{
			enemy.SetAnimationGoServerRpc();
		}
	}

	public void OnSecondarySkillHold(SpringManAI enemy)
	{
		HoldGo = true;
		if (Stopped(enemy) && enemy.IsOwner)
		{
			enemy.SetAnimationGoServerRpc();
		}
	}

	public void ReleaseSecondarySkill(SpringManAI enemy)
	{
		// Let go: its own rule takes over again (it freezes the moment someone is looking).
		HoldGo = false;
	}

	public bool IsAbleToMove(SpringManAI enemy)
	{
		return HoldGo || !Stopped(enemy);
	}

	public bool IsAbleToRotate(SpringManAI enemy)
	{
		return IsAbleToMove(enemy);
	}

	public float InteractRange(SpringManAI _)
	{
		return 1.5f;
	}

	public bool CanUseEntranceDoors(SpringManAI _)
	{
		return false;
	}

	public string GetSecondarySkillName(SpringManAI _)
	{
		return "(HOLD) Move while being watched";
	}
}

/// <summary>Keeps a driven CoilHead in chase mode, honours the right-click hold, and walks only when you walk.</summary>
[HarmonyPatch]
internal static class SpringManAIPatches
{
	private static readonly AccessTools.FieldRef<SpringManAI, bool> StoppingMovement = AccessTools.FieldRefAccess<SpringManAI, bool>("stoppingMovement");

	private static readonly AccessTools.FieldRef<SpringManAI, float> StopAndGoInterval = AccessTools.FieldRefAccess<SpringManAI, float>("stopAndGoMinimumInterval");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SpringManAI), nameof(SpringManAI.Update))]
	public static void UpdatePrefix(SpringManAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead)
		{
			return;
		}
		if (__instance.currentBehaviourStateIndex != (int)SpringManState.CHASING && __instance.IsOwner)
		{
			__instance.SwitchToBehaviourState((int)SpringManState.CHASING);
		}
		// No rest while you drive it: the rest is counted down by its AI, which is paused, so it would never end.
		__instance.timeSpentMoving = 0f;
		if (SpringManController.HoldGo)
		{
			// Every frame the game freezes it again if someone is looking; while held, it doesn't get to.
			StopAndGoInterval(__instance) = 1f;
			StoppingMovement(__instance) = false;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SpringManAI), nameof(SpringManAI.Update))]
	public static void UpdatePostfix(SpringManAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance) || __instance.isEnemyDead || __instance.creatureAnimator == null || StoppingMovement(__instance))
		{
			return;
		}
		// The game plays its walk at full speed whenever it may move; driven, only while you're moving. (Only the
		// animation: its own speed value is left alone, since the spring "boing" when it freezes plays only when
		// that value says it was moving fast.)
		float speed = Control.YouAreMoving ? (Control.YouAreSprinting ? 6f : 4.7f) : 0f;
		__instance.creatureAnimator.SetFloat("walkSpeed", speed);
	}
}
