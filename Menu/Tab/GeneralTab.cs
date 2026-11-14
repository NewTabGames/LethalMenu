using LethalMenu.Menu.Core;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

internal class GeneralTab : MenuTab
{
	private Vector2 scrollPos;

	private Texture2D avatar;

	public GeneralTab()
		: base("GeneralTab.Title")
	{
		GetImage("https://icyrelic.com/img/Avatar2.jpg", Avatar);
	}

	public void Avatar(Texture2D texture)
	{
		avatar = texture;
	}

	public override void Draw()
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		MenuContent();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void MenuContent()
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.Label("All support to IcyRelic and Dustin, the creators of Lethal Menu, as I am simply fixing it up. - TyGamer4", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
		GUILayout.Space(8f);
		GUILayout.BeginHorizontal();
		GUILayout.Label(avatar, GUILayout.Width(100f), GUILayout.Height(100f));
		GUILayout.Label("Thank you for using Lethal Menu.\n\nIf you have any suggestions please leave a comment on the forum post.\nAny bugs you find please provide some steps to recreate the issue and leave a comment.");
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		foreach (string change in Settings.Changelog.changes)
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			if (change.StartsWith("v"))
			{
				gUIStyle.fontStyle = FontStyle.Bold;
			}
			GUILayout.Label(change.StartsWith("v") ? ("Changelog " + change) : change, gUIStyle);
		}
		GUILayout.EndScrollView();
	}
}
