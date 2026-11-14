using System;
using System.Collections.Generic;
using System.Linq;
using LethalMenu.Menu.Core;
using LethalMenu.Types;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Popup;

internal class UnlockableManagerWindow(int id) : PopupMenu("UnlockableManager.Title", new Rect(50f, 50f, 562f, 225f), id)
{
	private Vector2 scrollPos = Vector2.zero;

	private string s_search = "";

	private List<Unlockable> unlockables = new List<Unlockable>();

	public override void DrawContent(int windowID)
	{
		if (LethalMenu.localPlayer == null || StartOfRound.Instance == null)
		{
			UI.Label("General.NullError", Settings.c_error);
			GUI.DragWindow();
			return;
		}
		if (unlockables.Count == 0)
		{
			unlockables = (from Unlockable u in Enum.GetValues(typeof(Unlockable))
				where u != Unlockable.OrangeSuit
				select u).ToList();
		}
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginHorizontal();
		UI.Textbox("General.Search", ref s_search);
		GUILayout.FlexibleSpace();
		UI.Button("UnlockableManager.UnlockAll", () =>
		{
			BuyAllUnlockables();
		});
		GUILayout.EndHorizontal();
		UI.ButtonGrid(unlockables, (Unlockable u) => u.GetItem().unlockableName, s_search, (Unlockable u) =>
		{
			BuyUnlockable(u);
		}, 3);
		GUILayout.EndScrollView();
		GUI.DragWindow();
	}

	private void BuyAllUnlockables()
	{
		if (!(HUDManager.Instance == null))
		{
			Enum.GetValues(typeof(Unlockable)).Cast<Unlockable>().ToList()
				.ForEach((Unlockable u) =>
				{
					BuyUnlockable(u, enabled: false);
				});
			HUDManager.Instance.DisplayTip("Lethal Menu", "Unlocked all unlockables!");
		}
	}

	public void BuyUnlockable(Unlockable unlockable, bool enabled = true)
	{
		if (!(StartOfRound.Instance == null) && !(HUDManager.Instance == null) && !(LethalMenu.terminal == null))
		{
			unlockable.Buy(LethalMenu.terminal.groupCredits);
			if (enabled)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "Unlocked " + unlockable.GetItem().unlockableName + "!");
			}
		}
	}
}
