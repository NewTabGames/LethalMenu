using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class Weight : Cheat
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerControllerB), "Update")]
	public static void WeightUpdate(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && __instance.playerClientId == LethalMenu.localPlayer.playerClientId)
		{
			__instance.carryWeight = (Hack.Weight.IsEnabled() ? 1f : GetHeldWeight(__instance));
		}
	}

	private static float GetHeldWeight(PlayerControllerB player)
	{
		float num = 1f;
		if (player.ItemSlots == null)
		{
			return num;
		}
		GrabbableObject[] itemSlots = player.ItemSlots;
		foreach (GrabbableObject grabbableObject in itemSlots)
		{
			if (!(grabbableObject == null) && !(grabbableObject.itemProperties == null))
			{
				num += Mathf.Clamp(grabbableObject.itemProperties.weight - 1f, 0f, 10f);
			}
		}
		return num;
	}
}
