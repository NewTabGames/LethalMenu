using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class Movement : Cheat
{
	public override void Update()
	{
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		if ((bool)StartOfRound.Instance && !(localPlayerController == null))
		{
			if (Settings.f_defaultMovementSpeed == -1f)
			{
				Settings.f_defaultMovementSpeed = localPlayerController.movementSpeed;
			}
			localPlayerController.movementSpeed = (Hack.SuperSpeed.IsEnabled() ? UnityEngine.Mathf.Max(Settings.f_movementSpeed, Settings.f_defaultMovementSpeed) : Settings.f_defaultMovementSpeed);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
	public static void PlayerLateUpdate(PlayerControllerB __instance)
	{
		if (!(LethalMenu.localPlayer == null) && LethalMenu.localPlayer.playerClientId == __instance.playerClientId)
		{
			__instance.movementSpeed = (Hack.SuperSpeed.IsEnabled() ? UnityEngine.Mathf.Max(Settings.f_movementSpeed, Settings.f_defaultMovementSpeed) : Settings.f_defaultMovementSpeed);
		}
	}
}
