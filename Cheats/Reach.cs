using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class Reach : Cheat
{
	public override void Update()
	{
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		if (!(localPlayerController == null))
		{
			if (Settings.f_defaultGrabDistance == -1f)
			{
				Settings.f_defaultGrabDistance = localPlayerController.grabDistance;
			}
			if (!Hack.LootThroughWalls.IsEnabled() && !Hack.InteractThroughWalls.IsEnabled())
			{
				localPlayerController.grabDistance = (Hack.Reach.IsEnabled() ? Settings.f_grabDistance : Settings.f_defaultGrabDistance);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
	public static void PlayerLateUpdate(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && LethalMenu.localPlayer.playerClientId == __instance.playerClientId && Hack.Reach.IsEnabled() && !Hack.LootThroughWalls.IsEnabled() && !Hack.InteractThroughWalls.IsEnabled())
		{
			__instance.grabDistance = (Hack.Reach.IsEnabled() ? Settings.f_grabDistance : Settings.f_defaultGrabDistance);
		}
	}
}
