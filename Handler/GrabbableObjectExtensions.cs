using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace LethalMenu.Handler;

public static class GrabbableObjectExtensions
{
	public static Bounds GetBounds(this GameObject gameObject)
	{
		Renderer componentInChildren = gameObject.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			return componentInChildren.bounds;
		}
		Collider componentInChildren2 = gameObject.GetComponentInChildren<Collider>();
		if (componentInChildren2 != null)
		{
			return componentInChildren2.bounds;
		}
		return new Bounds(Vector3.zero, Vector3.zero);
	}

	public static bool IsDefaultShipItem(this GrabbableObject GrabbableObject)
	{
		if (GrabbableObject == null)
		{
			return false;
		}
		string[] source = new string[2] { "ClipboardManual", "StickyNoteItem" };
		return Enumerable.Contains(source, GrabbableObject.name);
	}

	public static void PlaceOnDesk(this GrabbableObject GrabbableObject, float wait = 0.1f)
	{
		LethalMenu.Instance.StartCoroutine(GrabbableObject.PlaceOnDeskWait(wait));
	}

	public static IEnumerator PlaceOnDeskWait(this GrabbableObject GrabbableObject, float wait = 0.1f)
	{
		PlayerControllerB player = LethalMenu.players.Where((PlayerControllerB p) => p != null && !p.isPlayerDead).FirstOrDefault();
		if (!(LethalMenu.depositItemsDesk == null))
		{
			player.currentlyHeldObjectServer = GrabbableObject;
			LethalMenu.depositItemsDesk.PlaceItemOnCounter(player);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public static void ShootGunAsEnemy(this ShotgunItem shotgun, EnemyAI enemy)
	{
		shotgun.gunShootAudio.volume = 0.15f;
		shotgun.shotgunRayPoint = enemy.transform;
		shotgun.ShootGunAndSync(heldByPlayer: false);
	}
}
