using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalMenu;

/// <summary>
/// Makes outdated copies of the menu unusable. The lowest version still allowed is the text in
/// required-version.txt in the GitHub repo (e.g. "1.0.0"). To force everyone onto a new release: bump
/// Settings.version, publish the release, then put the same number in that file.
///
/// While outdated, the whole screen is black with a link to the repo, the menu can't be opened, no keybinds or
/// cheats run, and the website's commands are refused. If the check can't be made (no internet, GitHub down)
/// the menu is NOT locked, so nobody gets stuck because of a network problem.
/// </summary>
internal static class VersionLock
{
	internal const string RepoUrl = "https://github.com/NewTabGames/LethalMenu";

	private const string RequiredUrl = "https://raw.githubusercontent.com/NewTabGames/LethalMenu/main/required-version.txt";

	/// <summary>Checked once at launch, then every 10 minutes while the game is running.</summary>
	private const float RecheckEvery = 600f;

	private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15.0) };

	private static float nextCheck;

	private static bool lockApplied;

	private static volatile bool checking;

	/// <summary>The required version from GitHub when this copy is older than it; null when it's fine.</summary>
	private static volatile string requiredVersion;

	internal static bool Locked => requiredVersion != null;

	internal static string Message => "Outdated menu (" + Settings.version + "). Please update to " + requiredVersion + " or newer here:";

	/// <summary>Main thread, every frame: starts a check now and then (the download runs in the background).</summary>
	internal static void Tick()
	{
		if (Locked && !lockApplied)
		{
			// Just found out it's outdated: switch every feature off and close the menu. The cheats keep
			// updating, so they put the game's own values back (speed, jump...) like when they're toggled off.
			lockApplied = true;
			foreach (Hack hack in new System.Collections.Generic.List<Hack>(HackExtensions.ToggleFlags.Keys))
			{
				HackExtensions.ToggleFlags[hack] = false;
			}
			Settings.isMenuOpen = false;
			Debug.LogWarning("[LethalMenu] " + Message + " " + RepoUrl);
		}
		if (checking || Time.unscaledTime < nextCheck)
		{
			return;
		}
		nextCheck = Time.unscaledTime + RecheckEvery;
		checking = true;
		Task.Run(Check);
	}

	private static async Task Check()
	{
		try
		{
			// The query string gets around GitHub's few-minute cache when the file was just changed.
			string text = await Http.GetStringAsync(RequiredUrl + "?t=" + DateTime.UtcNow.Ticks).ConfigureAwait(false);
			string required = text.Trim();
			if (Version.TryParse(Clean(required), out Version need) && Version.TryParse(Clean(Settings.version), out Version have))
			{
				requiredVersion = have < need ? "v" + Clean(required) : null;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Couldn't check for a required update: " + ex.Message);
		}
		finally
		{
			checking = false;
		}
	}

	/// <summary>"v1.2" / "1.2.0" / " 1.2.0\n" to "1.2.0" (System.Version wants at least major.minor).</summary>
	private static string Clean(string version)
	{
		string v = (version ?? "").Trim().TrimStart('v', 'V');
		return v.Contains(".") ? v : v + ".0";
	}

	/// <summary>The black screen. Drawn last, over everything, with the cursor freed so the buttons can be clicked.</summary>
	internal static void DrawLockScreen()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		GUI.depth = -1000;
		Color old = GUI.color;
		GUI.color = Color.black;
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
		GUI.color = Color.white;
		GUIStyle text = new GUIStyle(GUI.skin.label)
		{
			fontSize = 26,
			alignment = TextAnchor.MiddleCenter,
			wordWrap = true
		};
		text.normal.textColor = Color.white;
		GUIStyle link = new GUIStyle(text) { fontSize = 22 };
		link.normal.textColor = new Color(0.55f, 0.75f, 1f);
		float w = Mathf.Min(900f, Screen.width - 40f);
		float x = (Screen.width - w) / 2f;
		float y = Screen.height / 2f - 110f;
		GUI.Label(new Rect(x, y, w, 70f), "Lethal Menu Reborn: " + Message, text);
		GUI.Label(new Rect(x, y + 75f, w, 40f), RepoUrl, link);
		GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 20 };
		if (GUI.Button(new Rect(Screen.width / 2f - 230f, y + 135f, 220f, 46f), "Open download page", button))
		{
			Application.OpenURL(RepoUrl);
		}
		if (GUI.Button(new Rect(Screen.width / 2f + 10f, y + 135f, 220f, 46f), "Quit game", button))
		{
			Application.Quit();
		}
		GUI.color = old;
	}
}
