using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using LethalMenu.Menu.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMenu.Util;

public class MenuUtil
{
	public static bool resizing = false;

	public static float maxWidth = (float)Screen.width - (float)Screen.width * 0.1f;

	public static float maxHeight = (float)Screen.height - (float)Screen.height * 0.1f;

	private static int oldWidth;

	private static int oldHeight;

	private static Coroutine LMUserCoroutine;

	private static bool SendLMUserMessage = true;

	public static float MouseX => Mouse.current.position.value.x;

	public static float MouseY => (float)Screen.height - Mouse.current.position.value.y;

	public static void BeginResizeMenu()
	{
		if (!resizing)
		{
			WarpCursor();
			resizing = true;
			oldWidth = Settings.i_menuWidth;
			oldHeight = Settings.i_menuHeight;
		}
	}

	public static void WarpCursor()
	{
		float x = HackMenu.Instance.windowRect.x + HackMenu.Instance.windowRect.width;
		float y = (float)Screen.height - (HackMenu.Instance.windowRect.y + HackMenu.Instance.windowRect.height);
		Mouse.current.WarpCursorPosition(new Vector2(x, y));
	}

	public static void ResizeMenu()
	{
		if (resizing)
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				resizing = false;
				Settings.Config.SaveConfig();
			}
			else if (Mouse.current.rightButton.wasPressedThisFrame)
			{
				resizing = false;
				Settings.i_menuWidth = oldWidth;
				Settings.i_menuHeight = oldHeight;
				Settings.Config.SaveConfig();
			}
			else
			{
				float num = HackMenu.Instance.windowRect.x + HackMenu.Instance.windowRect.width;
				float num2 = HackMenu.Instance.windowRect.y + HackMenu.Instance.windowRect.height;
				Settings.i_menuWidth = (int)Mathf.Clamp(MouseX - HackMenu.Instance.windowRect.x, 500f, maxWidth);
				Settings.i_menuHeight = (int)Mathf.Clamp(MouseY - HackMenu.Instance.windowRect.y, 250f, maxHeight);
				HackMenu.Instance.Resize();
			}
		}
	}

	public static void ShowCursor()
	{
		LethalMenu.localPlayer?.playerActions.Disable();
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public static void HideCursor()
	{
		if (LethalMenu.localPlayer == null)
		{
			if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
			{
				ShowCursor();
			}
		}
		else
		{
			LethalMenu.localPlayer?.playerActions.Enable();
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	/// <summary>Forgets coroutines/state owned by the destroyed menu object so a later load starts clean.</summary>
	internal static void ResetForEject()
	{
		LMUserCoroutine = null;
		SendLMUserMessage = true;
		resizing = false;
	}

	public static void StartLMUser()
	{
		if (LMUserCoroutine == null)
		{
			LMUserCoroutine = LethalMenu.Instance.StartCoroutine(LMUser());
		}
		else
		{
			SendLMUserMessage = true;
		}
	}

	private static IEnumerator LMUser()
	{
		while (!Settings.b_DisplayLMUsers || HUDManager.Instance == null || LethalMenu.localPlayer == null)
		{
			yield return new WaitForSeconds(10f);
		}
		HUDManager.Instance.Reflect().Invoke("AddTextMessageServerRpc", $"<size=0>{LethalMenu.localPlayer.playerSteamId}, {Settings.version}</size>");
		Regex s = new Regex("\\b\\d{17,19}\\b");
		Regex v = new Regex(",\\s*(v\\d+\\.\\d+\\.\\d+)");
		while (Settings.b_DisplayLMUsers)
		{
			if (SendLMUserMessage)
			{
				SendLMUserMessage = false;
				HUDManager.Instance.Reflect().Invoke("AddTextMessageServerRpc", $"<size=0>{LethalMenu.localPlayer.playerSteamId}, {Settings.version}</size>");
			}
			HUDManager.Instance.ChatMessageHistory.ToList().ForEach((string m) =>
			{
				if (s.Match(m).Success && v.Match(m).Success)
				{
					LethalMenu.Instance.LMUsers[s.Match(m).Value] = v.Match(m).Groups[1].Value;
				}
			});
			yield return new WaitForSeconds(15f);
		}
		LMUserCoroutine = null;
	}
}
