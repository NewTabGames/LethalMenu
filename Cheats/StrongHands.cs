using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class StrongHands : Cheat
{
	public override void Update()
	{
		if (!(LethalMenu.localPlayer == null))
		{
			GrabbableObject currentlyHeldObjectServer = LethalMenu.localPlayer.currentlyHeldObjectServer;
			LethalMenu.localPlayer.twoHanded = !(currentlyHeldObjectServer == null) && !Hack.StrongHands.IsEnabled() && currentlyHeldObjectServer.itemProperties.twoHanded;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
	public static void PlayerLateUpdate(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && __instance.playerClientId == LethalMenu.localPlayer.playerClientId)
		{
			GrabbableObject currentlyHeldObjectServer = __instance.currentlyHeldObjectServer;
			__instance.twoHanded = !(currentlyHeldObjectServer == null) && !Hack.StrongHands.IsEnabled() && currentlyHeldObjectServer.itemProperties.twoHanded;
		}
	}
}
