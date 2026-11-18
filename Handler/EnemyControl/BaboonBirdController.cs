using LethalMenu.Util;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class BaboonBirdController : IEnemyController<BaboonBirdAI>, IController
{
	public void OnDeath(BaboonBirdAI enemy)
	{
		if (!(enemy == null) && !(enemy.heldScrap == null))
		{
			DropHeldItemAndSync(enemy);
		}
	}

	// The camp position is no longer moved while you drive: it's shared by every baboon, so moving it sent all the
	// others wandering off and looting their own camp. A driven baboon's AI is paused, so it never drops scrap at
	// camp by itself anyway.

	/// <summary>
	/// Only its AI made a resting baboon get up, and that's paused while you drive, so one taken from camp kept
	/// sitting or sleeping while you moved it. Get it up, for everyone.
	/// </summary>
	public void Update(BaboonBirdAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy == null || !enemy.IsOwner || enemy.creatureAnimator == null)
		{
			return;
		}
		bool sitting = enemy.creatureAnimator.GetBool("sit");
		bool sleeping = enemy.creatureAnimator.GetBool("sleep");
		if (!sitting && !sleeping)
		{
			return;
		}
		enemy.creatureAnimator.SetBool("sit", false);
		enemy.creatureAnimator.SetBool("sleep", false);
		if (sitting)
		{
			enemy.EnemyGetUpServerRpc();
		}
		// Others only stop the sleep pose when it's back in its normal state.
		if (enemy.currentBehaviourStateIndex == 1)
		{
			enemy.SwitchToBehaviourState(0);
		}
		enemy.Reflect().SetValue("restingAtCamp", false);
		enemy.Reflect().SetValue("eyesClosed", false);
	}

	public void UsePrimarySkill(BaboonBirdAI enemy)
	{
		if (enemy == null)
		{
			return;
		}
		if (enemy.heldScrap is ShotgunItem shotgun)
		{
			shotgun.ShootGunAsEnemy(enemy);
			return;
		}
		GrabbableObject grabbableObject = enemy.FindNearbyItem(5f);
		if ((object)grabbableObject != null)
		{
			NetworkObject component = grabbableObject.GetComponent<NetworkObject>();
			if ((object)component != null)
			{
				GrabItemAndSync(enemy, component);
			}
		}
	}

	public void UseSecondarySkill(BaboonBirdAI enemy)
	{
		if (!(enemy == null) && !(enemy.heldScrap == null))
		{
			DropHeldItemAndSync(enemy);
		}
	}

	public string GetPrimarySkillName(BaboonBirdAI enemy)
	{
		return (enemy != null && enemy.heldScrap is ShotgunItem) ? "Shoot shotgun" : "Grab nearby item";
	}

	public string GetSecondarySkillName(BaboonBirdAI enemy)
	{
		return (enemy != null && enemy.heldScrap != null) ? "Drop item" : "";
	}

	public float InteractRange(BaboonBirdAI _)
	{
		return 5f;
	}

	public bool CanUseEntranceDoors(BaboonBirdAI _)
	{
		return false;
	}

	public static void GrabItemAndSync(BaboonBirdAI enemy, NetworkObject netitem)
	{
		if (!(netitem == null))
		{
			GrabScrap(enemy, netitem);
			enemy.GrabScrapServerRpc(netitem, (int)LethalMenu.localPlayer.playerClientId);
		}
	}

	public static void GrabScrap(BaboonBirdAI enemy, NetworkObject netitem)
	{
		if (enemy.heldScrap != null)
		{
			DropScrap(enemy, enemy.heldScrap.GetComponent<NetworkObject>(), enemy.heldScrap.GetItemFloorPosition());
		}
		GrabbableObject grabbableObject = (enemy.heldScrap = netitem.gameObject.GetComponent<GrabbableObject>());
		grabbableObject.parentObject = enemy.grabTarget;
		grabbableObject.hasHitGround = false;
		grabbableObject.GrabItemFromEnemy(enemy);
		grabbableObject.isHeldByEnemy = true;
		grabbableObject.EnablePhysics(enable: false);
	}

	public static void DropHeldItemAndSync(BaboonBirdAI enemy)
	{
		if (!(enemy.heldScrap == null))
		{
			// Game v73+: DropScrapServerRpc was replaced by DropScrapRpc; reuse the game's own drop+sync logic.
			HarmonyLib.AccessTools.Method(typeof(BaboonBirdAI), "DropHeldItemAndSync")?.Invoke(enemy, new object[1] { true });
		}
	}

	public static void DropScrap(BaboonBirdAI enemy, NetworkObject item, Vector3 targetFloorPosition)
	{
		if (!(enemy.heldScrap == null))
		{
			if (enemy.heldScrap.isHeld)
			{
				enemy.heldScrap.DiscardItemFromEnemy();
				enemy.heldScrap.isHeldByEnemy = false;
				enemy.heldScrap = null;
				return;
			}
			enemy.heldScrap.parentObject = null;
			enemy.heldScrap.transform.SetParent(StartOfRound.Instance.propsContainer, worldPositionStays: true);
			enemy.heldScrap.EnablePhysics(enable: true);
			enemy.heldScrap.fallTime = 0f;
			enemy.heldScrap.startFallingPosition = enemy.heldScrap.transform.parent.InverseTransformPoint(enemy.heldScrap.transform.position);
			enemy.heldScrap.targetFloorPosition = enemy.heldScrap.transform.parent.InverseTransformPoint(targetFloorPosition);
			enemy.heldScrap.floorYRot = -1;
			enemy.heldScrap.DiscardItemFromEnemy();
			enemy.heldScrap.isHeldByEnemy = false;
			enemy.heldScrap = null;
		}
	}
}
