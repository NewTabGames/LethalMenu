using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch(typeof(ItemDropship), "ShipLandedAnimationEvent")]
internal class OpenDropShipLand : Cheat
{
	[HarmonyPostfix]
	public static void ShipLandedAnimationEvent(ItemDropship __instance)
	{
		if (Hack.OpenDropShipLand.IsEnabled() && !(__instance == null) && !__instance.shipDoorsOpened)
		{
			__instance.OpenShipServerRpc();
		}
	}
}
