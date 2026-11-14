namespace LethalMenu.Cheats;

internal class UnlimitedBattery : Cheat
{
	public override void Update()
	{
		if (!Hack.UnlimitedBattery.IsEnabled() || LethalMenu.localPlayer == null)
		{
			return;
		}
		GrabbableObject[] itemSlots = LethalMenu.localPlayer.ItemSlots;
		foreach (GrabbableObject grabbableObject in itemSlots)
		{
			if (!(grabbableObject == null) && grabbableObject.itemProperties.requiresBattery)
			{
				grabbableObject.insertedBattery.charge = 1f;
				grabbableObject.SyncBatteryServerRpc(100);
			}
		}
	}
}
