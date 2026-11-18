using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class AntiGhostGirl : Cheat
{
	[HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
	[HarmonyPostfix]
	public static void ChoosePlayerToHaunt(DressGirlAI __instance)
	{
		if (Hack.AntiGhostGirl.IsEnabled() && !(__instance == null) && __instance.hauntingLocalPlayer)
		{
			__instance.hauntingPlayer = null;
		}
	}

	[HarmonyPatch(typeof(DressGirlAI), "BeginChasing")]
	[HarmonyPostfix]
	public static void BeginChasing(DressGirlAI __instance)
	{
		if (Hack.AntiGhostGirl.IsEnabled() && !(__instance == null) && __instance.hauntingLocalPlayer)
		{
			__instance.hauntingPlayer = null;
		}
	}

	// With no one to haunt, the game's own Update and AI step read hauntingPlayer and threw an error every frame.
	// A girl this cheat took the target from just stays idle instead.
	[HarmonyPatch(typeof(DressGirlAI), nameof(DressGirlAI.Update))]
	[HarmonyPrefix]
	public static bool Update(DressGirlAI __instance)
	{
		return !(Hack.AntiGhostGirl.IsEnabled() && __instance.hauntingPlayer == null);
	}

	[HarmonyPatch(typeof(DressGirlAI), nameof(DressGirlAI.DoAIInterval))]
	[HarmonyPrefix]
	public static bool DoAIInterval(DressGirlAI __instance)
	{
		return !(Hack.AntiGhostGirl.IsEnabled() && __instance.hauntingPlayer == null);
	}
}
