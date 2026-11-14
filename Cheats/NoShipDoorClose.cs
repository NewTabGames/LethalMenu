using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LethalMenu.Handler;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class NoShipDoorClose : Cheat
{
	public static Dictionary<string, bool> Triggered = new Dictionary<string, bool>();

	[HarmonyPostfix]
	[HarmonyPatch(typeof(InteractTrigger), "Interact")]
	public static void Interact(InteractTrigger __instance, Transform playerTransform)
	{
		if (!(__instance == null) && !(playerTransform == null) && !(__instance.transform == null))
		{
			Triggered[__instance.transform.name] = true;
			LethalMenu.Instance.StartCoroutine(WaitForTriggerFinish(__instance, __instance.animationWaitTime));
		}
	}

	private static IEnumerator WaitForTriggerFinish(InteractTrigger __instance, float animationWaitTime)
	{
		yield return new WaitForSeconds(animationWaitTime);
		Triggered[__instance.transform.name] = false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StartOfRound), "SetShipDoorsOverheatClientRpc")]
	public static bool SetShipDoorsOverheatClientRpc(StartOfRound __instance)
	{
		if (Hack.NoShipDoorClose.IsEnabled() && PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			return false;
		}
		if (Hack.NoShipDoorClose.IsEnabled() && !PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "This is host only :C");
			return true;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(HangarShipDoor), "PlayDoorAnimation")]
	public static bool PlayDoorAnimation(bool closed)
	{
		InteractTrigger interactTrigger = LethalMenu.interactTriggers.FirstOrDefault((InteractTrigger i) => i != null && i.transform.parent != null && i.transform.parent.name == "StartButton");
		if (interactTrigger == null || LethalMenu.shipDoor == null)
		{
			return false;
		}
		bool value = default;
		if (((bool)LethalMenu.localPlayer && Triggered.TryGetValue(interactTrigger.transform.name, out value)) & value)
		{
			return true;
		}
		if (Hack.NoShipDoorClose.IsEnabled() && PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			return false;
		}
		return true;
	}
}
