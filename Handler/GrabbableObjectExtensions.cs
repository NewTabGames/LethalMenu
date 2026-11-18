using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Util;
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
		// Shoot from the monster's head toward where you aim, without touching the gun's own muzzle point. It
		// used to be swapped for the monster permanently: later shots from that gun measured distance from the
		// monster, and once the monster was gone they threw errors and couldn't hurt you any more.
		Vector3 position = enemy.eye != null ? enemy.eye.position : enemy.transform.position + Vector3.up;
		Vector3 forward = Cheats.EnemyControl.AimPoint(30f) - position;
		forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : enemy.transform.forward;
		float oldVolume = shotgun.gunShootAudio != null ? shotgun.gunShootAudio.volume : 1f;
		if (shotgun.gunShootAudio != null)
		{
			shotgun.gunShootAudio.volume = 0.15f;
		}
		shotgun.ShootGun(position, forward);
		if (shotgun.gunShootAudio != null)
		{
			shotgun.gunShootAudio.volume = oldVolume;
		}
		shotgun.Reflect().SetValue("localClientSendingShootGunRPC", true); // the game's flag: don't fire it twice here
		shotgun.ShootGunServerRpc(position, forward);
	}
}
