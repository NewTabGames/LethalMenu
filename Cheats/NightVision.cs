using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class NightVision : Cheat
{
	public override void Update()
	{
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		if (!(localPlayerController == null))
		{
			if (Settings.f_nvIntensity == -1f)
			{
				Settings.f_nvIntensity = localPlayerController.nightVision.intensity;
			}
			if (Settings.f_nvRange == -1f)
			{
				Settings.f_nvRange = localPlayerController.nightVision.range;
			}
			localPlayerController.nightVision.enabled = Hack.NightVision.IsEnabled() || localPlayerController.isInsideFactory;
			localPlayerController.nightVision.intensity = (Hack.NightVision.IsEnabled() ? Settings.f_nvIntensity : Settings.f_defaultNightVisionIntensity);
			localPlayerController.nightVision.range = (Hack.NightVision.IsEnabled() ? Settings.f_nvRange : Settings.f_defaultNightVisionRange);
		}
	}
}
