using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler;

public static class HoarderBugAIExtensions
{
	public static void StealAllItems(this HoarderBugAI bug)
	{
		bug.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
		LethalMenu.Instance.StartCoroutine(StealItems(bug));
	}

	private static IEnumerator StealItems(HoarderBugAI bug)
	{
		List<NetworkObject> items = LethalMenu.items.FindAll((GrabbableObject i) => !i.isHeld && !i.isPocketed && !i.isInShipRoom && i.isInFactory).ConvertAll((GrabbableObject i) => i.NetworkObject);
		foreach (NetworkObject obj in items)
		{
			yield return new WaitForSeconds(0.2f);
			bug.GrabItemServerRpc(obj);
			bug.DropItemServerRpc(obj, bug.nestPosition, droppedInNest: true);
		}
	}
}
