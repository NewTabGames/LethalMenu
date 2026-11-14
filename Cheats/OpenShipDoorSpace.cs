using HarmonyLib;

namespace LethalMenu.Cheats;

internal class OpenShipDoorSpace : Cheat
{
	[HarmonyPatch(typeof(HangarShipDoor), "Update")]
	public class HangarShipDoorUpdatePatch
	{
		[HarmonyPrefix]
		public static bool Prefix(HangarShipDoor __instance)
		{
			if (Hack.OpenShipDoorSpace.IsEnabled() && !__instance.buttonsEnabled && StartOfRound.Instance.inShipPhase)
			{
				__instance.SetDoorButtonsEnabled(doorButtonsEnabled: true);
			}
			else if (!Hack.OpenShipDoorSpace.IsEnabled() && __instance.buttonsEnabled && StartOfRound.Instance.inShipPhase)
			{
				__instance.SetDoorButtonsEnabled(doorButtonsEnabled: false);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(StartOfRound), "TeleportPlayerInShipIfOutOfRoomBounds")]
	public class StartOfRoundTeleportPlayerInShipIfOutOfRoomBoundsPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(StartOfRound __instance)
		{
			if (Hack.OpenShipDoorSpace.IsEnabled())
			{
				return false;
			}
			return true;
		}
	}
}
