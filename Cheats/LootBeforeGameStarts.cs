using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;

namespace LethalMenu.Cheats;

[HarmonyPatch]
public class LootBeforeGameStarts
{
	private static Dictionary<GrabbableObject, bool> ModifiedItems = new Dictionary<GrabbableObject, bool>();

	[HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
	[HarmonyPrefix]
	public static void BeginGrabObject(PlayerControllerB __instance)
	{
		if (Hack.LootBeforeGameStarts.IsEnabled())
		{
			GrabbableObject value = __instance.Reflect().GetValue<GrabbableObject>("currentlyGrabbingObject");
			if (!(value == null) && !(value.itemProperties == null) && !value.itemProperties.canBeGrabbedBeforeGameStart && !GameNetworkManager.Instance.gameHasStarted)
			{
				ModifiedItems.Add(value, value.itemProperties.canBeGrabbedBeforeGameStart);
				value.itemProperties.canBeGrabbedBeforeGameStart = true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControllerB), "DiscardHeldObject")]
	[HarmonyPrefix]
	public static void DiscardHeldObject(PlayerControllerB __instance)
	{
		if (ModifiedItems.ContainsKey(__instance.currentlyHeldObjectServer))
		{
			if (ModifiedItems.TryGetValue(__instance.currentlyHeldObjectServer, out var value))
			{
				__instance.currentlyHeldObjectServer.itemProperties.canBeGrabbedBeforeGameStart = value;
			}
			ModifiedItems.Remove(__instance.currentlyHeldObjectServer);
		}
	}
}
