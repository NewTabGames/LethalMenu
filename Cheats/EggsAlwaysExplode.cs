namespace LethalMenu.Cheats;

internal class EggsAlwaysExplode : Cheat
{
	public override void Update()
	{
		if (Hack.EggsAlwaysExplode.IsEnabled() && !Hack.EggsNeverExplode.IsEnabled() && LethalMenu.localPlayer != null && LethalMenu.localPlayer?.currentlyHeldObjectServer is StunGrenadeItem stunGrenadeItem && stunGrenadeItem != null && stunGrenadeItem.explodeSFX.name == "EasterEggPop")
		{
			stunGrenadeItem.SetExplodeOnThrowClientRpc(explode: true);
		}
	}
}
