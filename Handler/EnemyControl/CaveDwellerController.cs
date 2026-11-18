using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class CaveDwellerController : IEnemyController<CaveDwellerAI>, IController
{
	public void UsePrimarySkill(CaveDwellerAI enemy)
	{
		// A non-host waits for the host's answer before it starts growing; don't ask twice meanwhile.
		if (!enemy.adultContainer.activeSelf && Time.time - transformRequestedAt > 3f)
		{
			transformRequestedAt = Time.time;
			TransformIntoAdult(enemy);
		}
	}

	private float transformRequestedAt = -10f;

	/// <summary>Right click was pressed: leap as soon as its scream is done.</summary>
	private bool leapRequested;

	private float leapRequestedAt;

	/// <summary>Who the requested leap is aimed at.</summary>
	private PlayerControllerB? leapTarget;

	/// <summary>The requested leap has started; calm it down once it's over.</summary>
	private bool sawLeap;

	/// <summary>
	/// Adult: leap. In the game the adult only kills while leaping (touching a player during the leap), and only
	/// its own AI decided when to leap, so a driven adult couldn't kill anyone. This starts the game's own attack:
	/// a short scream, then the leap forward where you face; anyone it hits during the leap is killed.
	/// </summary>
	public void UseSecondarySkill(CaveDwellerAI enemy)
	{
		if (!enemy.adultContainer.activeSelf || enemy.Reflect().GetValue<bool>("leaping") || enemy.Reflect().GetValue<bool>("inTransformingAnimation"))
		{
			return;
		}
		// Its attack code stops when it has no target; whoever is chosen or nearest (it still kills anyone it hits).
		// Not your own body unless you picked yourself as the target.
		PlayerControllerB? target = Control.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 80f, requireLineOfSight: false);
		if (target == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No one to leap at");
			return;
		}
		leapTarget = target;
		enemy.targetPlayer = target;
		enemy.Reflect().SetValue("chasingAfterLeap", false);
		enemy.Reflect().SetValue("screaming", false);
		enemy.Reflect().SetValue("leaping", false);
		if (enemy.currentBehaviourStateIndex != 3)
		{
			enemy.SwitchToBehaviourState(3);
		}
		leapRequested = true;
		leapRequestedAt = Time.time;
	}

	public void Update(CaveDwellerAI enemy, bool isAIControlled)
	{
		if (isAIControlled)
		{
			leapRequested = false;
			sawLeap = false;
			return;
		}
		bool leaping = enemy.Reflect().GetValue<bool>("leaping");
		// Left in its attack state after the leap, it kept screaming and leaping at the target on its own, killing
		// whoever it touched. Once your leap (and its cooldown) is over, put it back to plain walking.
		if (sawLeap)
		{
			if (enemy.currentBehaviourStateIndex != 3)
			{
				sawLeap = false;
			}
			else if (!leaping || enemy.Reflect().GetValue<bool>("chasingAfterLeap"))
			{
				sawLeap = false;
				enemy.SwitchToBehaviourState(1);
			}
			return;
		}
		if (!leapRequested)
		{
			return;
		}
		if (enemy.currentBehaviourStateIndex != 3)
		{
			leapRequested = false;
			return;
		}
		if (Time.time - leapRequestedAt > 3f)
		{
			leapRequested = false;
			if (!leaping)
			{
				enemy.SwitchToBehaviourState(1);
			}
			return;
		}
		if (leaping)
		{
			leapRequested = false;
			sawLeap = true;
			Control.Dash(enemy.transform.forward, Mathf.Max(enemy.leapSpeed, 8f), Mathf.Clamp(enemy.leapTime, 0.3f, 2f));
			return;
		}
		// Entering its attack state picks the closest player, which could be your own body. Keep it on your target.
		if (leapTarget != null && enemy.targetPlayer != leapTarget)
		{
			enemy.targetPlayer = leapTarget;
		}
		// Keep the scream before the leap short.
		if (enemy.Reflect().GetValue<float>("screamTimer") > 0.35f)
		{
			enemy.Reflect().SetValue("screamTimer", 0.35f);
		}
	}

	public void OnReleaseControl(CaveDwellerAI enemy)
	{
		leapRequested = false;
		sawLeap = false;
		leapTarget = null;
	}

	/// <summary>Can't be steered while growing up (or while a player carries the baby).</summary>
	public bool IsAbleToMove(CaveDwellerAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public bool IsAbleToRotate(CaveDwellerAI enemy)
	{
		return !enemy.inSpecialAnimation;
	}

	public string GetPrimarySkillName(CaveDwellerAI enemy)
	{
		return (!enemy.adultContainer.activeSelf) ? "Transform To Adult" : "";
	}

	public string GetSecondarySkillName(CaveDwellerAI enemy)
	{
		return enemy.adultContainer.activeSelf ? "Leap (kills whoever it hits)" : "";
	}

	public bool CanUseEntranceDoors(CaveDwellerAI _)
	{
		return true;
	}

	public float InteractRange(CaveDwellerAI _)
	{
		return 5f;
	}

	/// <summary>
	/// On the host, run the game's own TransformIntoAdult(). On a non-host client, TurnIntoAdultServerRpc's
	/// ClientRpc skips the host (`if (!base.IsServer)`), leaving the host's copy stuck as a baby. Instead, two
	/// zero-damage hits in state 0 push the server's growthMeter past 1 (0.6 each), so the host's next BabyUpdate
	/// runs TransformIntoAdult() on the host and sends TurnIntoAdultClientRpc to everyone.
	/// </summary>
	public void TransformIntoAdult(CaveDwellerAI enemy)
	{
		if (enemy.IsServer)
		{
			enemy.Reflect().Invoke("TransformIntoAdult");
		}
		else
		{
			enemy.HitEnemyServerRpc(0, -1, playHitSFX: false);
			enemy.HitEnemyServerRpc(0, -1, playHitSFX: false);
		}
	}
}

/// <summary>
/// On the host, the adult re-picks the nearest player as its target every 0.3 s and hands itself to them. While you
/// drive it, that took ownership away from you and swapped your leap target.
/// </summary>
[HarmonyPatch]
internal static class CaveDwellerAIPatches
{
	private static readonly AccessTools.FieldRef<CaveDwellerAI, float> ChangeOwnershipInterval = AccessTools.FieldRefAccess<CaveDwellerAI, float>("changeOwnershipInterval");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CaveDwellerAI), nameof(CaveDwellerAI.Update))]
	public static void Update(CaveDwellerAI __instance)
	{
		if (Control.IsDrivenByYou(__instance))
		{
			ChangeOwnershipInterval(__instance) = 1f;
		}
	}
}
