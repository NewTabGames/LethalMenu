using GameNetcodeStuff;
using LethalMenu.Util;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class HoarderBugController : IEnemyController<HoarderBugAI>, IController
{
	/// <summary>Right click toggles this. While on, the bug stays in its angry chase pose and hurts players it touches.</summary>
	private bool attacking;

	public void OnTakeControl(HoarderBugAI enemy)
	{
		attacking = false;
	}

	public void OnReleaseControl(HoarderBugAI enemy)
	{
		if (attacking && enemy != null && !enemy.isEnemyDead)
		{
			// Let go while attacking: the bug keeps hunting its target on its own for a while.
			PlayerControllerB target = PickTarget(enemy);
			if (target != null)
			{
				enemy.angryAtPlayer = target;
				enemy.watchingPlayer = target;
				enemy.targetPlayer = target;
				enemy.angryTimer = 30f;
			}
		}
		attacking = false;
	}

	/// <summary>The player chosen with T / the website, else the nearest other player, else whoever it is already angry at.</summary>
	private static PlayerControllerB PickTarget(HoarderBugAI enemy)
	{
		return Cheats.EnemyControl.ValidChosenTarget ?? ControlUtil.GetNearestPlayer(enemy, 60f, requireLineOfSight: false) ?? enemy.angryAtPlayer ?? GameNetworkManager.Instance?.localPlayerController;
	}

	public void Update(HoarderBugAI enemy, bool isAIControlled)
	{
		if (enemy == null)
		{
			return;
		}
		if (attacking)
		{
			// Keep the game's anger aimed at the target. Also runs while the AI drives (F9),
			// so the AI's own chase goes after the chosen player.
			PlayerControllerB target = PickTarget(enemy);
			enemy.angryAtPlayer = target;
			enemy.watchingPlayer = target;
			enemy.targetPlayer = target;
			if (enemy.angryTimer < 5f)
			{
				enemy.angryTimer = 5f;
			}
			if (!enemy.IsBehaviourState(BugState.CHASING_PLAYER))
			{
				enemy.SetBehaviourState(BugState.CHASING_PLAYER);
			}
		}
		else if (!isAIControlled)
		{
			enemy.angryTimer = 0f;
			if (enemy.IsBehaviourState(BugState.CHASING_PLAYER))
			{
				enemy.SetBehaviourState(BugState.SEARCHING_FOR_ITEMS);
			}
		}
	}

	private static void StopAttacking(HoarderBugAI enemy)
	{
		enemy.angryTimer = 0f;
		enemy.angryAtPlayer = null;
		if (enemy.IsBehaviourState(BugState.CHASING_PLAYER))
		{
			enemy.SetBehaviourState(BugState.SEARCHING_FOR_ITEMS);
		}
	}

	public void OnDeath(HoarderBugAI enemy)
	{
		attacking = false;
		if (enemy != null && enemy.heldItem != null && enemy.heldItem.itemGrabbableObject != null && enemy.heldItem.itemGrabbableObject.TryGetComponent<NetworkObject>(out var component))
		{
			DropItemAndCallDropRPC(enemy, component, droppedInNest: false);
		}
	}

	private static bool Holding(HoarderBugAI enemy)
	{
		return enemy != null && enemy.heldItem != null && enemy.heldItem.itemGrabbableObject != null;
	}

	/// <summary>Left click: grab a nearby item; if already holding one, shoot it (shotgun) or drop it.</summary>
	public void UsePrimarySkill(HoarderBugAI enemy)
	{
		if (enemy == null)
		{
			return;
		}
		if (Holding(enemy))
		{
			if (enemy.heldItem.itemGrabbableObject is ShotgunItem shotgun)
			{
				shotgun.ShootGunAsEnemy(enemy);
			}
			else if (enemy.heldItem.itemGrabbableObject.TryGetComponent<NetworkObject>(out var held))
			{
				DropItemAndCallDropRPC(enemy, held, droppedInNest: false);
			}
			return;
		}
		GrabbableObject grabbableObject = enemy.FindNearbyItem(5f);
		if (grabbableObject != null && grabbableObject.TryGetComponent<NetworkObject>(out var component))
		{
			GrabItemAndCallGrabRPC(enemy, component);
		}
		else
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No item close enough to grab");
		}
	}

	/// <summary>Right click: start / stop attacking.</summary>
	public void UseSecondarySkill(HoarderBugAI enemy)
	{
		if (enemy == null)
		{
			return;
		}
		attacking = !attacking;
		if (!attacking)
		{
			StopAttacking(enemy);
		}
	}

	public string GetPrimarySkillName(HoarderBugAI enemy)
	{
		if (!Holding(enemy))
		{
			return "Grab nearby item";
		}
		return (enemy.heldItem.itemGrabbableObject is ShotgunItem) ? "Shoot shotgun" : "Drop item";
	}

	public string GetSecondarySkillName(HoarderBugAI enemy)
	{
		return attacking ? "Stop attacking (attacking now)" : "Start attacking";
	}

	public float InteractRange(HoarderBugAI _)
	{
		return 1.5f;
	}

	public bool CanUseEntranceDoors(HoarderBugAI _)
	{
		return false;
	}

	public static void GrabItemAndCallGrabRPC(HoarderBugAI enemy, NetworkObject item)
	{
		GrabItem(enemy, item);
		enemy.Reflect().SetValue("sendingGrabOrDropRPC", true);
		enemy.GrabItemServerRpc(item);
	}

	public static void GrabItem(HoarderBugAI enemy, NetworkObject item)
	{
		if (enemy.Reflect().GetValue<bool>("sendingGrabOrDropRPC"))
		{
			enemy.Reflect().SetValue("sendingGrabOrDropRPC", false);
			return;
		}
		if (enemy.heldItem != null)
		{
			DropItem(enemy, enemy.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), enemy.heldItem.itemGrabbableObject.GetItemFloorPosition());
		}
		enemy.targetItem = null;
		GrabbableObject component = item.gameObject.GetComponent<GrabbableObject>();
		HoarderBugAI.HoarderBugItems.Add(new HoarderBugItem(component, HoarderBugItemStatus.Owned, enemy.nestPosition));
		enemy.heldItem = HoarderBugAI.HoarderBugItems[HoarderBugAI.HoarderBugItems.Count - 1];
		component.parentObject = enemy.grabTarget;
		component.hasHitGround = false;
		component.GrabItemFromEnemy(enemy);
		component.EnablePhysics(enable: false);
		HoarderBugAI.grabbableObjectsInMap.Remove(component.gameObject);
	}

	private static void DropItemAndCallDropRPC(HoarderBugAI enemy, NetworkObject dropItemNetworkObject, bool droppedInNest = true)
	{
		Vector3 targetFloorPosition = RoundManager.Instance.RandomlyOffsetPosition(enemy.heldItem.itemGrabbableObject.GetItemFloorPosition(), 1.2f, 0.4f);
		DropItem(enemy, dropItemNetworkObject, targetFloorPosition);
		enemy.Reflect().SetValue("sendingGrabOrDropRPC", true);
		enemy.DropItemServerRpc(dropItemNetworkObject, targetFloorPosition, droppedInNest);
	}

	public static void DropItem(HoarderBugAI enemy, NetworkObject item, Vector3 targetFloorPosition, bool droppingInNest = true)
	{
		if (enemy.Reflect().GetValue<bool>("sendingGrabOrDropRPC"))
		{
			enemy.Reflect().SetValue("sendingGrabOrDropRPC", false);
		}
		else if (enemy.heldItem != null)
		{
			GrabbableObject itemGrabbableObject = enemy.heldItem.itemGrabbableObject;
			itemGrabbableObject.parentObject = null;
			itemGrabbableObject.transform.SetParent(StartOfRound.Instance.propsContainer, worldPositionStays: true);
			itemGrabbableObject.EnablePhysics(enable: true);
			itemGrabbableObject.fallTime = 0f;
			itemGrabbableObject.startFallingPosition = itemGrabbableObject.transform.parent.InverseTransformPoint(itemGrabbableObject.transform.position);
			itemGrabbableObject.targetFloorPosition = itemGrabbableObject.transform.parent.InverseTransformPoint(targetFloorPosition);
			itemGrabbableObject.floorYRot = -1;
			itemGrabbableObject.DiscardItemFromEnemy();
			enemy.heldItem = null;
			if (!droppingInNest)
			{
				HoarderBugAI.grabbableObjectsInMap.Add(itemGrabbableObject.gameObject);
			}
		}
	}
}
