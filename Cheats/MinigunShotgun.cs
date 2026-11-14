using UnityEngine.InputSystem;

namespace LethalMenu.Cheats;

internal class MinigunShotgun : Cheat
{
	public override void Update()
	{
		if (Hack.MinigunShotgun.IsEnabled() && !(LethalMenu.localPlayer == null))
		{
			ShotgunItem shotgunItem = LethalMenu.localPlayer.currentlyHeldObjectServer as ShotgunItem;
			if (!(shotgunItem == null) && Mouse.current.leftButton.isPressed)
			{
				shotgunItem.ShootGunServerRpc(LethalMenu.localPlayer.transform.position - LethalMenu.localPlayer.gameplayCamera.transform.up * 0.45f, LethalMenu.localPlayer.gameplayCamera.transform.forward);
			}
		}
	}
}
