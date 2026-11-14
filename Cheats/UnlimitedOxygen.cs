using HarmonyLib;

namespace LethalMenu.Cheats;

internal class UnlimitedOxygen
{
	[HarmonyPatch(typeof(StartOfRound), "Update")]
	public class StartOfRoundUpdatePatch
	{
		[HarmonyPostfix]
		public static void Prefix(StartOfRound __instance)
		{
			if (Hack.UnlimitedOxygen.IsEnabled() && (bool)LethalMenu.localPlayer)
			{
				__instance.drowningTimer = 1f;
			}
		}
	}
}
