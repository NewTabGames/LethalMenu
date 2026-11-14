using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class EnemyControlPatches
{
	[HarmonyPatch(typeof(HUDManager), "CanPlayerScan")]
	[HarmonyPostfix]
	public static void CanPlayerScan(ref bool __result)
	{
		if (Hack.EnemyControl.IsEnabled())
		{
			__result = false;
		}
	}
}
