namespace LethalMenu.Types;

public static class UnlockableExtensions
{
	public static UnlockableItem GetItem(this Unlockable unlockable)
	{
		return StartOfRound.Instance.unlockablesList.unlockables[(int)unlockable];
	}

	/// <summary>False when this game version doesn't have that unlockable (an older version, or before a lobby loads).</summary>
	public static bool Exists(this Unlockable unlockable)
	{
		int index = (int)unlockable;
		System.Collections.Generic.List<UnlockableItem> list = StartOfRound.Instance?.unlockablesList?.unlockables;
		return list != null && index >= 0 && index < list.Count && list[index] != null;
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
