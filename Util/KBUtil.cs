using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LethalMenu.Util;

public class KBUtil
{
	internal class KBCallback
	{
		private Hack hack;

		public KBCallback(Hack hack)
		{
			this.hack = hack;
		}

		public IEnumerator Invoke(ButtonControl btn)
		{
			hack.SetKeyBind(btn);
			yield return new WaitForSeconds(0.1f);
			hack.SetWaiting(b: false);
			Settings.Config.SaveConfig();
		}
	}

	private const float WaitSeconds = 15f;

	public static void BeginChangeKeyBind(Hack hack, params Action[] callbacks)
	{
		hack.SetWaiting(b: true);
		// Typing must not go into a text box (e.g. the keybind search) while a key is being captured.
		GUIUtility.keyboardControl = 0;
		LethalMenu.Instance.StartCoroutine(TryGetPressedKeyCoroutine(hack, new KBCallback(hack).Invoke, callbacks));
	}

	private static IEnumerator TryGetPressedKeyCoroutine(Hack hack, Func<ButtonControl, IEnumerator> callback, params Action[] otherCallbacks)
	{
		yield return new WaitForSeconds(0.25f);
		float startTime = Time.time;
		ButtonControl btn = null;
		bool cancelled = false;
		while (btn == null && !cancelled && Time.time - startTime <= WaitSeconds)
		{
			Keyboard keyboard = Keyboard.current;
			if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
			{
				cancelled = true; // Escape cancels instead of binding Escape
				break;
			}
			ButtonControl pressed = null;
			try
			{
				pressed = GetPressedBtn();
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Key capture: " + ex.Message);
			}
			if (pressed != null)
			{
				btn = pressed;
			}
			yield return null;
		}
		if (btn != null)
		{
			LethalMenu.Instance.StartCoroutine(callback(btn));
		}
		else
		{
			// Timed out or cancelled: never leave the hack stuck on "Waiting" (that blocks every hotkey).
			hack.SetWaiting(b: false);
		}
		for (int i = 0; i < otherCallbacks.Length; i++)
		{
			otherCallbacks[i]?.Invoke();
		}
	}

	public static ButtonControl GetPressedBtn()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard != null)
		{
			foreach (KeyControl key in keyboard.allKeys)
			{
				// The key list can contain empty slots for keys this keyboard layout does not have.
				if (key != null && key.wasPressedThisFrame)
				{
					return key;
				}
			}
		}
		Mouse mouse = Mouse.current;
		if (mouse == null)
		{
			return null;
		}
		ButtonControl[] buttons = new ButtonControl[5] { mouse.leftButton, mouse.rightButton, mouse.middleButton, mouse.forwardButton, mouse.backButton };
		foreach (ButtonControl button in buttons)
		{
			if (button != null && button.wasPressedThisFrame)
			{
				return button;
			}
		}
		return null;
	}
}
