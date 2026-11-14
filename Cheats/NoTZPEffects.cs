using LethalMenu.Util;

namespace LethalMenu.Cheats;

internal class NoTZPEffects : Cheat
{
	public override void Update()
	{
		if (Hack.NoTZPEffects.IsEnabled() && LethalMenu.localPlayer != null && HUDManager.Instance != null && LethalMenu.localPlayer != null && LethalMenu.localPlayer?.currentlyHeldObjectServer is TetraChemicalItem tetraChemicalItem && tetraChemicalItem != null)
		{
			LethalMenu.localPlayer.drunknessInertia = 0f;
			LethalMenu.localPlayer.increasingDrunknessThisFrame = false;
			HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
			tetraChemicalItem?.Reflect().SetValue("emittingGas", false);
			LethalMenu.localPlayer.increasingDrunknessThisFrame = false;
		}
	}
}
