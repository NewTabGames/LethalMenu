using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
internal class SuperJump : Cheat
{
	[HarmonyPostfix]
	public static void PlayerJump(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && !(__instance == null) && !(LethalMenu.localPlayer != __instance))
		{
			__instance.jumpForce = (Hack.SuperJump.IsEnabled() ? UnityEngine.Mathf.Max(Settings.f_jumpForce, Settings.f_defaultJumpForce) : Settings.f_defaultJumpForce);
		}
	}
}
