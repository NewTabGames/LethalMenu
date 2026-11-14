using LethalMenu.Util;

namespace LethalMenu.Cheats;

internal class UnlimitedTZP : Cheat
{
	public override void Update()
	{
		if (Hack.UnlimitedTZP.IsEnabled() && LethalMenu.localPlayer != null && LethalMenu.localPlayer?.currentlyHeldObjectServer is TetraChemicalItem tetraChemicalItem && tetraChemicalItem != null)
		{
			tetraChemicalItem?.Reflect().SetValue("fuel", 1f);
		}
	}
}
