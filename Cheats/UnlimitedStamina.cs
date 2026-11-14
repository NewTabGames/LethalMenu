using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class UnlimitedStamina : Cheat
{
	public override void Update()
	{
		if (Hack.UnlimitedStamina.IsEnabled())
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			if (!(localPlayerController == null))
			{
				localPlayerController.sprintMeter = 1f;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
	public static void PlayerLateUpdate(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && LethalMenu.localPlayer.playerClientId == __instance.playerClientId && Hack.UnlimitedStamina.IsEnabled())
		{
			__instance.sprintMeter = 1f;
			if (__instance.sprintMeterUI != null)
			{
				__instance.sprintMeterUI.fillAmount = 1f;
			}
		}
	}
}
