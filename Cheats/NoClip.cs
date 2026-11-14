using GameNetcodeStuff;
using LethalMenu.Components;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class NoClip : Cheat
{
	private KBInput movement = null;

	public override void Update()
	{
		if (!LethalMenu.localPlayer)
		{
			return;
		}
		Collider component = LethalMenu.localPlayer.GetComponent<CharacterController>();
		if (Hack.NoClip.IsEnabled())
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			if (movement == null)
			{
				movement = localPlayerController.gameObject.AddComponent<KBInput>();
			}
			component.enabled = false;
			localPlayerController.transform.transform.position = movement.transform.position;
		}
		else
		{
			component.enabled = true;
			Object.Destroy(movement);
			movement = null;
		}
	}
}
