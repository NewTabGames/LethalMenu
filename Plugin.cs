using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMenu;

/// <summary>
/// BepInEx entry point. Drop LethalMenu.dll into BepInEx\plugins and it loads itself on startup.
/// The same DLL still works with SharpMonoInjector (LethalMenu.Loader.Init); this class is simply never touched then.
/// </summary>
[BepInPlugin(Guid, "Lethal Menu", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
	public const string Guid = "lethalmenu.bepinex";

	internal static ConfigEntry<bool> LoadOnStartup;

	internal static ConfigEntry<Key> ReloadKey;

	private void Awake()
	{
		LoadOnStartup = Config.Bind("General", "LoadOnStartup", true, "Load the menu automatically when the game starts. If false, the menu stays off until you press ReloadKey.");
		ReloadKey = Config.Bind("General", "ReloadKey", Key.Pause, "Key that loads the menu again after it has been ejected. (Ejecting is done from the menu: Settings > Eject Menu, or the Unload Menu keybind, Pause by default.)");
		Loader.RunningUnderBepInEx = true;

		// Own, hidden host object: independent of the BepInEx manager object and of scene changes.
		GameObject host = new GameObject("LethalMenu.Host")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		Object.DontDestroyOnLoad(host);
		host.AddComponent<PluginHost>();
		Logger.LogInfo("Lethal Menu plugin ready. LoadOnStartup=" + LoadOnStartup.Value + ", ReloadKey=" + ReloadKey.Value);
	}
}

/// <summary>Loads the menu once the input system is up, and reloads it on ReloadKey after an eject.</summary>
internal class PluginHost : MonoBehaviour
{
	private const float StartupDelay = 1f;

	private bool startupHandled;

	private void Update()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard == null)
		{
			return;
		}
		if (!startupHandled)
		{
			if (Time.realtimeSinceStartup < StartupDelay)
			{
				return;
			}
			startupHandled = true;
			if (Plugin.LoadOnStartup.Value)
			{
				TryLoad();
			}
			return;
		}
		if (Loader.IsLoaded || Ejector.Ejecting || Plugin.ReloadKey.Value == Key.None)
		{
			return;
		}
		PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
		if (player != null && (player.isTypingChat || player.inTerminalMenu))
		{
			return;
		}
		if (keyboard[Plugin.ReloadKey.Value].wasPressedThisFrame)
		{
			TryLoad();
		}
	}

	private static void TryLoad()
	{
		try
		{
			Loader.Init();
			Debug.Log("[LethalMenu] Loaded.");
		}
		catch (System.Exception ex)
		{
			Debug.LogError("[LethalMenu] Failed to load: " + ex);
		}
	}
}
