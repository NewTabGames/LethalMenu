using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class CaveDwellerController : IEnemyController<CaveDwellerAI>, IController
{
	public void UsePrimarySkill(CaveDwellerAI enemy)
	{
		if (!enemy.adultContainer.activeSelf)
		{
			TransformIntoAdult(enemy);
		}
	}

	/// <summary>Right click was pressed: leap as soon as its scream is done.</summary>
	private bool leapRequested;

	private float leapRequestedAt;

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
		enemy.targetPlayer = Cheats.EnemyControl.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 80f, requireLineOfSight: false) ?? LethalMenu.localPlayer;
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
		if (!leapRequested || isAIControlled)
		{
			return;
		}
		if (enemy.currentBehaviourStateIndex != 3 || Time.time - leapRequestedAt > 3f)
		{
			leapRequested = false;
			return;
		}
		if (enemy.Reflect().GetValue<bool>("leaping"))
		{
			leapRequested = false;
			Cheats.EnemyControl.Dash(enemy.transform.forward, Mathf.Max(enemy.leapSpeed, 8f), Mathf.Clamp(enemy.leapTime, 0.3f, 2f));
			return;
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

	public void TransformIntoAdult(CaveDwellerAI enemy)
	{
		enemy.SwitchToBehaviourStateOnLocalClient(1);
		enemy.TurnIntoAdultServerRpc();
		enemy.Reflect().Invoke("StartTransformationAnim");
	}
}
