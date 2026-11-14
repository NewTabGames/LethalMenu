using HarmonyLib;
using LethalMenu.Util;
using UnityEngine.InputSystem;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class UnlimitedPresents : Cheat
{
	public static bool stuckpresent;

	public override void Update()
	{
		if (Hack.UnlimitedPresents.IsEnabled() && LethalMenu.localPlayer.currentlyHeldObjectServer is GiftBoxItem && stuckpresent && Keyboard.current.gKey.isPressed)
		{
			Hack.DeleteHeldItem.Execute();
			Hack.DropAllItems.Execute();
			stuckpresent = false;
		}
	}

	[HarmonyPatch(typeof(GrabbableObject), "DestroyObjectInHand")]
	[HarmonyPrefix]
	public static bool DestroyObjectInHand()
	{
		if (Hack.UnlimitedPresents.IsEnabled() && LethalMenu.localPlayer.currentlyHeldObjectServer is GiftBoxItem)
		{
			stuckpresent = true;
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(GrabbableObject), "Start")]
	[HarmonyPostfix]
	public static void Start(GrabbableObject __instance)
	{
		if (Hack.UnlimitedPresents.IsEnabled() && LethalMenu.localPlayer.currentlyHeldObjectServer is GiftBoxItem obj && stuckpresent)
		{
			obj.Reflect().SetValue("hasUsedGift", false);
		}
	}
}
