using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LethalMenu.Themes;

public class Theme
{
	public static AssetBundle AssetBundle;

	public static string name { get; private set; }

	public static GUISkin Skin { get; private set; }

	public static void Initialize()
	{
		SetTheme(string.IsNullOrEmpty(name) ? "Default" : name);
	}

	public static void SetTheme(string t)
	{
		LoadTheme(name = (ThemeExists(t) ? t : "Default"));
	}

	private static bool ThemeExists(string t)
	{
		return Assembly.GetExecutingAssembly().GetManifestResourceStream("LethalMenu.Resources.Theme." + t + ".skin") != null;
	}

	private static AssetBundle LoadAssetBundle(string r)
	{
		return AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(r));
	}

	public static string[] GetThemes()
	{
		return (from name in (from r in Assembly.GetExecutingAssembly().GetManifestResourceNames()
				where r.StartsWith("LethalMenu.Resources.Theme.") && r.EndsWith(".skin")
				select r).Select((string r) =>
			{
				int length = "LethalMenu.Resources.Theme.".Length;
				int length2 = ".skin".Length;
				return r.Substring(length, r.Length - length2 - length);
			})
			orderby name
			select name).ToArray();
	}

	/// <summary>Frees the theme asset bundle so the menu can be loaded again later (used when ejecting).</summary>
	public static void Unload()
	{
		AssetBundle?.Unload(unloadAllLoadedObjects: true);
		AssetBundle = null;
		Skin = null;
	}

	private static void LoadTheme(string t)
	{
		AssetBundle?.Unload(unloadAllLoadedObjects: true);
		AssetBundle = null;
		Skin = null;
		AssetBundle = LoadAssetBundle("LethalMenu.Resources.Theme." + t + ".skin");
		if (AssetBundle == null)
		{
			Debug.LogError("[ERROR] Failed to load theme file => LethalMenu.Resources.Theme." + t + ".skin");
		}
		Skin = AssetBundle.LoadAllAssets<GUISkin>().FirstOrDefault();
		if (!(Skin == null))
		{
			Debug.Log("Loaded Theme " + t);
		}
	}
}
