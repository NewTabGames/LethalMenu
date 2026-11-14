using System.Collections.Generic;
using LethalMenu.Language;
using UnityEngine;

namespace LethalMenu.Menu.Core;

internal class PopupMenu : MenuFragment
{
	public Rect windowRect;

	public bool isOpen = false;

	public string name;

	private string localization;

	public int id;

	protected List<MenuTab> menuTabs = new List<MenuTab>();

	protected int selectedTab = 0;

	public PopupMenu(string name, Rect size, int id)
	{
		localization = name;
		windowRect = size;
		this.id = id;
		LocalizeName();
	}

	public void LocalizeName()
	{
		name = Localization.Localize(localization);
	}

	public void Draw()
	{
		if (isOpen)
		{
			windowRect = GUILayout.Window(id, windowRect, DrawContent, name);
		}
	}

	public virtual void DrawContent(int windowID)
	{
	}
}
