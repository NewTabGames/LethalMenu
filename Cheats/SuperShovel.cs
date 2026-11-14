using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class SuperShovel : Cheat
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Shovel), "HitShovel")]
	public static void HitShovel(Shovel __instance)
	{
		__instance.shovelHitForce = ((!Hack.SuperShovel.IsEnabled()) ? 1 : 1000);
	}
}
