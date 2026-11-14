using System.Collections.Generic;
using System.Linq;
using LethalMenu.Menu.Popup;
using LethalMenu.Menu.Tab;
using LethalMenu.Themes;
using UnityEngine;

namespace LethalMenu.Menu.Core;

internal class HackMenu : MenuFragment
{
	private Vector2 scrollPos = Vector2.zero;

	public Rect windowRect = new Rect(50f, 50f, 700f, 450f);

	public PopupMenu MoonManagerWindow = new MoonManagerWindow(1);

	public PopupMenu UnlockableManagerWindow = new UnlockableManagerWindow(2);

	public PopupMenu ItemManagerWindow = new ItemManagerWindow(3);

	public PopupMenu FirstSetupManagerWindow = new FirstSetupManagerWindow(4);

	public PopupMenu LootManagerWindow = new LootManagerWindow(5);

	public PopupMenu WeatherManagerWindow = new WeatherManagerWindow(6);

	public PopupMenu SuitManagerWindow = new SuitManagerWindow(7);

	public static List<MenuTab> menuTabs = new List<MenuTab>();

	private int selectedTab = 0;

	public float contentWidth;

	public float contentHeight;

	public int spaceFromTop = 60;

	public int spaceFromLeft = 10;

	private static HackMenu instance;

	public static HackMenu Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new HackMenu();
			}
			return instance;
		}
	}

	public HackMenu()
	{
		instance = this;
		menuTabs.Add(new SettingsTab());
		menuTabs.Add(new GeneralTab());
		menuTabs.Add(new SelfTab());
		menuTabs.Add(new VisualsTab());
		menuTabs.Add(new TrollTab());
		menuTabs.Add(new PlayersTab());
		menuTabs.Add(new EnemyTab());
		menuTabs.Add(new ServerTab());
		menuTabs.Add(new DebugTab()); // always there now; Debug Mode only switches the game's own test menu
		Resize();
		selectedTab = menuTabs.IndexOf(menuTabs.Find((MenuTab x) => x is GeneralTab));
	}

	/// <summary>The Debug tab is always shown now, whatever Debug Mode is set to.</summary>
	public void ToggleDebugTab(bool enabled)
	{
		if (!menuTabs.Any((MenuTab t) => t is DebugTab))
		{
			menuTabs.Add(new DebugTab());
		}
	}

	public void Resize()
	{
		windowRect.width = Settings.i_menuWidth;
		windowRect.height = Settings.i_menuHeight;
		contentWidth = windowRect.width - (float)(spaceFromLeft * 2);
		contentHeight = windowRect.height - (float)spaceFromTop;
	}

	public void ResetMenuSize()
	{
		Settings.i_menuFontSize = 14;
		Settings.i_menuWidth = 810;
		Settings.i_menuHeight = 410;
		Settings.i_sliderWidth = 100;
		Settings.i_textboxWidth = 85;
		Settings.Config.SaveConfig();
	}

	public void Stylize()
	{
		GUI.skin = Theme.Skin;
		GUI.color = Color.white;
		GUI.skin.label.fontSize = Settings.i_menuFontSize;
		GUI.skin.button.fontSize = Settings.i_menuFontSize;
		GUI.skin.toggle.fontSize = Settings.i_menuFontSize;
		GUI.skin.box.fontSize = Settings.i_menuFontSize;
		GUI.skin.textField.fontSize = Settings.i_menuFontSize;
		GUI.skin.horizontalSlider.fontSize = Settings.i_menuFontSize;
		GUI.skin.horizontalSliderThumb.fontSize = Settings.i_menuFontSize;
		GUI.skin.verticalSlider.fontSize = Settings.i_menuFontSize;
		GUI.skin.verticalSliderThumb.fontSize = Settings.i_menuFontSize;
		GUI.skin.customStyles.Where((GUIStyle x) => x.name == "TabBtn").First().fontSize = Settings.i_menuFontSize;
		GUI.skin.customStyles.Where((GUIStyle x) => x.name == "SelectedTab").First().fontSize = Settings.i_menuFontSize;
		Resize();
	}

	public void Draw()
	{
		if (Settings.isFirstLaunch || Settings.isMenuOpen)
		{
			Stylize();
			if (Settings.isFirstLaunch)
			{
				FirstSetupManagerWindow.Draw();
				return;
			}
			GUI.color = new Color(1f, 1f, 1f, Settings.f_menuAlpha);
			windowRect = GUILayout.Window(0, windowRect, DrawContent, "Lethal Menu Reborn " + Settings.version);
			UnlockableManagerWindow.Draw();
			ItemManagerWindow.Draw();
			MoonManagerWindow.Draw();
			LootManagerWindow.Draw();
			WeatherManagerWindow.Draw();
			SuitManagerWindow.Draw();
			GUI.color = Color.white;
		}
		else if (GUIUtility.keyboardControl != 0)
		{
			// Menu closed: no text box may keep the keyboard.
			GUIUtility.keyboardControl = 0;
		}
	}

	private void DrawContent(int windowID)
	{
		GUI.color = new Color(1f, 1f, 1f, 0.1f);
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 20,
			fontStyle = FontStyle.Bold
		};
		string text = "Developed By IcyRelic, and Dustin";
		GUI.Label(new Rect(windowRect.width - gUIStyle.CalcSize(new GUIContent(text)).x - 10f, windowRect.height - gUIStyle.CalcSize(new GUIContent(text)).y - 10f, gUIStyle.CalcSize(new GUIContent(text)).x, gUIStyle.CalcSize(new GUIContent(text)).y), text, gUIStyle);
		GUI.color = new Color(1f, 1f, 1f, Settings.f_menuAlpha);
		GUILayout.BeginVertical();
		GUILayout.BeginArea(new Rect(0f, 25f, windowRect.width, 25f), (GUIStyle)"Toolbar");
		GUILayout.BeginHorizontal();
		menuTabs.ForEach((MenuTab x) =>
		{
			x.LocalizeName();
		});
		selectedTab = GUILayout.Toolbar(selectedTab, menuTabs.Select((MenuTab x) => x.name).ToArray(), "TabBtn");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		GUILayout.Space(spaceFromTop);
		GUILayout.BeginArea(new Rect(spaceFromLeft, spaceFromTop, windowRect.width - (float)spaceFromLeft, contentHeight - 15f));
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginHorizontal();
		menuTabs[selectedTab].Draw();
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		GUI.color = Color.white;
		ReleaseTextBoxFocus();
		GUI.DragWindow();
	}

	/// <summary>
	/// IMGUI text boxes keep the keyboard until another control is clicked. Also let go of it on Enter or Escape,
	/// or when clicking an empty part of the menu (clicks on controls are already used up by then).
	/// </summary>
	internal static void ReleaseTextBoxFocus()
	{
		Event e = Event.current;
		if (GUIUtility.keyboardControl == 0 || e == null)
		{
			return;
		}
		bool confirmKey = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape);
		if (confirmKey || e.type == EventType.MouseDown)
		{
			GUIUtility.keyboardControl = 0;
			if (confirmKey)
			{
				e.Use();
			}
		}
	}
}
