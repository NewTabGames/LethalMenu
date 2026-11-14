using HarmonyLib;
using UnityEngine.InputSystem;

namespace LethalMenu.Cheats;

/// <summary>
/// Stealth Mode (Settings.b_StealthMode): nothing about the menu shows on this screen and nothing on this keyboard
/// reaches it. The menu window can't be opened, hotkeys are ignored, the yellow "Lethal Menu" notifications and the
/// monster-control help box are hidden. Features themselves still work and are driven from the website only.
/// Leaving it: the website's Stealth switch, or the emergency keys Left Ctrl + Left Alt + Insert on this PC.
/// </summary>
[HarmonyPatch]
internal class StealthMode : Cheat
{
	internal static bool Active => Settings.b_StealthMode;

	private static bool wasActive;

	/// <summary>Called every frame by LethalMenu, also on the main menu (cheats only update inside a lobby).</summary>
	internal static void Tick()
	{
		Keyboard keyboard = Keyboard.current;
		if (Active && keyboard != null && keyboard.leftCtrlKey.isPressed && keyboard.leftAltKey.isPressed && keyboard.insertKey.wasPressedThisFrame)
		{
			Settings.b_StealthMode = false; // emergency way out if the website can't be reached
		}
		if (Active)
		{
			CloseEverything();
		}
		if (Active != wasActive)
		{
			wasActive = Active;
			Settings.Config.SaveConfig(); // remembered across restarts, whichever side changed it
			if (!Active)
			{
				HUDManager.Instance?.DisplayTip("Lethal Menu", "Stealth Mode is off");
			}
		}
	}

	/// <summary>Closes the menu window and its pop-up windows, giving the mouse back to the game.</summary>
	private static void CloseEverything()
	{
		if (Settings.isMenuOpen)
		{
			Hack.OpenMenu.Execute();
		}
	}

	// Every notification the menu shows uses the "Lethal Menu" header.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(HUDManager), nameof(HUDManager.DisplayTip))]
	public static bool DisplayTip(string headerText)
	{
		return !(Active && headerText == "Lethal Menu");
	}
}
