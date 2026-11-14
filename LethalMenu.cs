using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Language;
using LethalMenu.Manager;
using LethalMenu.Menu.Core;
using LethalMenu.Menu.Popup;
using LethalMenu.Themes;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LethalMenu;

public class LethalMenu : MonoBehaviour
{
	private List<Cheat> cheats;

	public static List<GrabbableObject> items = new List<GrabbableObject>();

	public static List<Landmine> landmines = new List<Landmine>();

	public static List<Turret> turrets = new List<Turret>();

	public static List<EntranceTeleport> doors = new List<EntranceTeleport>();

	public static List<PlayerControllerB> players = new List<PlayerControllerB>();

	public static List<EnemyAI> enemies = new List<EnemyAI>();

	public static List<SteamValveHazard> steamValves = new List<SteamValveHazard>();

	public static List<TerminalAccessibleObject> bigDoors = new List<TerminalAccessibleObject>();

	public static List<TerminalAccessibleObject> allTerminalObjects = new List<TerminalAccessibleObject>();

	public static List<DoorLock> doorLocks = new List<DoorLock>();

	public static List<ShipTeleporter> teleporters = new List<ShipTeleporter>();

	public static List<InteractTrigger> interactTriggers = new List<InteractTrigger>();

	public static List<SpikeRoofTrap> spikeRoofTraps = new List<SpikeRoofTrap>();

	public static List<GameObject> vainShrouds = new List<GameObject>();

	public static List<AnimatedObjectTrigger> animatedTriggers = new List<AnimatedObjectTrigger>();

	public static List<EnemyVent> enemyVents = new List<EnemyVent>();

	public static List<VehicleController> vehicles = new List<VehicleController>();

	public static List<LocalVolumetricFog> fogs = new List<LocalVolumetricFog>();

	public static List<Volume> volumes = new List<Volume>();

	public static ItemDropship itemDropship;

	public static HangarShipDoor shipDoor;

	public static DepositItemsDesk depositItemsDesk;

	public static BreakerBox breaker;

	public static MineshaftElevatorController mineshaftElevator;

	public static PlayerControllerB localPlayer;

	public static QuickMenuManager quickMenuManager;

	public static Terminal terminal;

	public static int selectedPlayer = -1;

	public int fps;

	public static Harmony harmony;

	private HackMenu menu;

	private static LethalMenu instance;

	public static LethalMenu Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new LethalMenu();
			}
			return instance;
		}
	}

	/// <summary>The live menu instance, or null (unlike <see cref="Instance"/>, never creates one).</summary>
	internal static LethalMenu Current => instance;

	internal static void ClearInstance()
	{
		instance = null;
	}

	public void Start()
	{
		instance = this;
		try
		{
			Initialize();
		}
		catch (Exception ex)
		{
			Settings.debugMessage = ex.Message + "\n" + ex.StackTrace;
		}
	}

	private void Initialize()
	{
		Localization.Initialize();
		Theme.Initialize();
		HarmonyPatching();
		LoadCheats();
		ObjectManager.CollectObjects();
		StartCoroutine(FPSCounter());
		Remote.RemoteControl.Start();
	}

	private void HarmonyPatching()
	{
		harmony = new Harmony("LethalMenu");
		Harmony.DEBUG = false;
		Type[] types = Loader.GetLoadableTypes();
		foreach (Type type in types)
		{
			if (type.IsDefined(typeof(HarmonyPatch), inherit: false))
			{
				try
				{
					new PatchClassProcessor(harmony, type).Patch();
				}
				catch
				{
					Debug.LogWarning("Skipping patch in " + type.FullName);
				}
			}
		}
	}

	private void LoadCheats()
	{
		try
		{
			Settings.Changelog.ReadChanges();
			cheats = new List<Cheat>();
			menu = new HackMenu();
			foreach (Type item in from t in Loader.GetLoadableTypes()
				where string.Equals(t.Namespace, "LethalMenu.Cheats", StringComparison.Ordinal) && t.IsSubclassOf(typeof(Cheat))
				select t)
			{
				cheats.Add((Cheat)Activator.CreateInstance(item));
			}
			Settings.Config.SaveDefaultConfig();
			Settings.Config.LoadConfig();
			Settings.EnsureBoosts();
		}
		catch (Exception ex)
		{
			Settings.debugMessage = ex.Message;
		}
	}

	public void FixedUpdate()
	{
		try
		{
			if ((bool)StartOfRound.Instance)
			{
				cheats.ForEach((Cheat cheat) =>
				{
					cheat.FixedUpdate();
				});
			}
		}
		catch (Exception ex)
		{
			Settings.debugMessage = "Msg: " + ex.Message + "\nSrc: " + ex.Source + "\n" + ex.StackTrace;
		}
	}

	public void Update()
	{
		if (!StartOfRound.Instance || StartOfRound.Instance.inShipPhase)
		{
			Settings.v_savedLocation = Vector3.zero;
		}
		if (!Ejector.Ejecting)
		{
			Remote.RemoteControl.Tick();
			try
			{
				Cheats.StealthMode.Tick();
			}
			catch (Exception ex)
			{
				Settings.debugMessage = "Stealth: " + ex.Message;
			}
		}
		try
		{
			// No keybinds while ejecting; the cheats still update so they can restore the game's values.
			// None in Stealth Mode either: only the website drives the menu then.
			if (!Ejector.Ejecting && !Settings.b_StealthMode)
			{
				foreach (Hack value in Enum.GetValues(typeof(Hack)))
				{
					if (!StartOfRound.Instance || !(localPlayer != null) || (!localPlayer.isTypingChat && !localPlayer.quickMenuManager.isMenuOpen && !localPlayer.inTerminalMenu))
					{
						if (value.HasKeyBind() && value.GetKeyBind().wasPressedThisFrame && !value.IsAnyHackWaiting())
						{
							value.Execute();
						}
						if (!StartOfRound.Instance)
						{
							return;
						}
					}
				}
			}
			if (!StartOfRound.Instance || cheats == null)
			{
				return;
			}
			// One failing cheat must not stop the others (matters most while ejecting).
			foreach (Cheat cheat in cheats)
			{
				try
				{
					cheat.Update();
				}
				catch (Exception ex)
				{
					Settings.debugMessage = "Msg: " + ex.Message + "\nSrc: " + ex.Source + "\n" + ex.StackTrace;
				}
			}
		}
		catch (Exception ex)
		{
			Settings.debugMessage = "Msg: " + ex.Message + "\nSrc: " + ex.Source + "\n" + ex.StackTrace;
		}
	}

	public IEnumerator FPSCounter()
	{
		while (true)
		{
			fps = (int)(1f / Time.deltaTime);
			yield return new WaitForSeconds(1f);
		}
	}

	public void OnGUI()
	{
		if (Ejector.Ejecting)
		{
			return;
		}
		try
		{
			if (Event.current.type == EventType.Repaint)
			{
				// No watermark; only the FPS counter, and only when it is enabled in Settings.
				if (Settings.b_FPSCounter)
				{
					VisualUtil.DrawString(new Vector2(5f, 2f), $"FPS: {fps}", Settings.c_primary, centered: false, alignMiddle: false, bold: true, forceOnScreen: false, 14);
				}
				if (MenuUtil.resizing && !Settings.b_StealthMode)
				{
					VisualUtil.DrawString(new Vector2(Screen.width / 2, 35f), Localization.Localize(new string[3]
					{
						"SettingsTab.ResizeTitle",
						"SettingsTab.ResizeConfirm",
						$"{HackMenu.Instance.windowRect.width}x{HackMenu.Instance.windowRect.height}"
					}, newLine: true), Settings.c_playerESP, centered: true, alignMiddle: true, bold: true, forceOnScreen: true, 22);
					MenuUtil.ResizeMenu();
				}
				if (Settings.DebugMode && !Settings.b_StealthMode)
				{
					VisualUtil.DrawString(new Vector2(5f, 20f), "[DEBUG MODE]", new RGBAColor(50, 205, 50, 1f), centered: false, alignMiddle: false, bold: false, forceOnScreen: true, 10);
					VisualUtil.DrawString(new Vector2(10f, 65f), new RGBAColor(255, 195, 0, 1f).AsString(Settings.debugMessage), centered: false, alignMiddle: false, bold: false, forceOnScreen: false, 22);
				}
				if ((bool)StartOfRound.Instance)
				{
					foreach (Cheat cheat in cheats)
					{
						try
						{
							cheat.OnGui();
						}
						catch (Exception ex)
						{
							Settings.debugMessage = "Msg: " + ex.Message + "\nSrc: " + ex.Source + "\n" + ex.StackTrace;
						}
					}
				}
			}
			if (!Settings.b_StealthMode)
			{
				menu.Draw(); // the menu window, its pop-ups and the first-launch window
			}
		}
		catch (Exception ex)
		{
			Settings.debugMessage = "Msg: " + ex.Message + "\nSrc: " + ex.Source + "\n" + ex.StackTrace;
		}
	}

	public void Unload()
	{
		Ejector.Eject();
	}
}
