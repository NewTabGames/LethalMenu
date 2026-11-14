namespace LethalMenu.Types;

public static class UnlockableExtensions
{
	public static UnlockableItem GetItem(this Unlockable unlockable)
	{
		return StartOfRound.Instance.unlockablesList.unlockables[(int)unlockable];
	}

	public static void SetLocked(this Unlockable unlockable, bool locked)
	{
		UnlockableItem item = unlockable.GetItem();
		item.alreadyUnlocked = locked;
		item.hasBeenUnlockedByPlayer = locked;
	}

	public static void Buy(this Unlockable unlockable, int credits)
	{
		unlockable.SetLocked(locked: false);
		StartOfRound.Instance.BuyShipUnlockableServerRpc((int)unlockable, credits);
		StartOfRound.Instance.SyncShipUnlockablesServerRpc();
	}
}
