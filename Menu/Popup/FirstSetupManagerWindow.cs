using System;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace LethalMenu.Menu.Popup;

internal class FirstSetupManagerWindow : PopupMenu
{
	private bool disableBtns = false;

	public FirstSetupManagerWindow(int id)
		: base("FirstSetup.Title", new Rect(Screen.width / 2 - 150, (float)(Screen.height / 2) - 100f, 300f, 160f), id)
	{
		isOpen = true;
	}

	public override void DrawContent(int windowID)
	{
		if (disableBtns)
		{
			GUI.enabled = false;
		}
		windowRect.x = Screen.width / 2 - 150;
		windowRect.y = (float)(Screen.height / 2) - 100f;
		UI.Label("FirstSetup.Welcome");
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		UI.Label("FirstSetup.SelectKeybind");
		UI.Label("FirstSetup.Keybind", GetMenuKeybindName() ?? "");
		GUILayout.Space(20f);
		string label = (Hack.OpenMenu.IsWaiting() ? "General.Waiting" : "FirstSetup.ClickToChange");
		UI.Actions(new UIButton(label, () =>
		{
			disableBtns = true;
			KBUtil.BeginChangeKeyBind(Hack.OpenMenu, () =>
			{
				disableBtns = false;
			});
		}));
		GUILayout.EndVertical();
		GUILayout.Space(20f);
		UI.Actions(new UIButton("FirstSetup.Complete", () =>
		{
			Settings.isFirstLaunch = false;
			Settings.Config.SaveConfig();
		}));
		if (disableBtns)
		{
			GUI.enabled = true;
		}
		GUI.DragWindow();
	}

	public static string GetMenuKeybindName()
	{
		ButtonControl keyBind = Hack.OpenMenu.GetKeyBind();
		string result;
		if (Hack.OpenMenu.HasKeyBind())
		{
			result = ((keyBind.GetType() == typeof(KeyControl)) ? ((KeyControl)keyBind).keyCode.ToString() : keyBind.displayName);
		}
		else
		{
			result = "None";
		}
		return result;
	}
}
