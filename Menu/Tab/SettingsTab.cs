using System;
using System.Collections.Generic;
using System.Linq;
using LethalMenu.Cheats;
using LethalMenu.Handler;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Themes;
using LethalMenu.Types;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace LethalMenu.Menu.Tab;

internal class SettingsTab : MenuTab
{
	private string s_kbError = "";

	private string s_tierColorError = "";

	private string s_kbSearch = "";

	private Vector2 scrollPos = Vector2.zero;

	private Vector2 scrollPos2 = Vector2.zero;

	private string s_bgColor = Settings.c_background.GetHexCode();

	private string s_primaryColor = Settings.c_primary.GetHexCode();

	private string s_crosshairColor = Settings.c_crosshair.GetHexCode();

	private string s_hackhighlightColor = Settings.c_hackhighlight.GetHexCode();

	private string s_menuText = Settings.c_menuText.GetHexCode();

	private string s_objectESPColor = Settings.c_objectESP.GetHexCode();

	private string s_playerESPColor = Settings.c_playerESP.GetHexCode();

	private string s_bodyESPColor = Settings.c_bodyESP.GetHexCode();

	private string s_enemyESPColor = Settings.c_enemyESP.GetHexCode();

	private string s_landmineESPColor = Settings.c_landmineESP.GetHexCode();

	private string s_turretESPColor = Settings.c_turretESP.GetHexCode();

	private string s_doorESPColor = Settings.c_entranceExitESP.GetHexCode();

	private string s_doorLockESPColor = Settings.c_doorLockESP.GetHexCode();

	private string s_valveESPColor = Settings.c_steamHazardESP.GetHexCode();

	private string s_bigDoorESPColor = Settings.c_bigDoorESP.GetHexCode();

	private string s_shipESPColor = Settings.c_shipESP.GetHexCode();

	private string s_breakerESPColor = Settings.c_breakerESP.GetHexCode();

	private string s_spikeRoofTrapESPColor = Settings.c_spikeRoofTrapESP.GetHexCode();

	private string s_mineshaftElevatorESPColor = Settings.c_mineshaftElevatorESP.GetHexCode();

	private string s_enemyVentESPColor = Settings.c_enemyVentESP.GetHexCode();

	private string s_vainShroudESPColor = Settings.c_vainShroudESP.GetHexCode();

	private string s_itemDropShipESPColor = Settings.c_itemDropShipESP.GetHexCode();

	private string s_cruiserESPColor = Settings.c_CruiserESP.GetHexCode();

	private string s_chamsColor = Settings.c_chams.GetHexCode();

	private string s_objectChamsColor = Settings.c_objectChams.GetHexCode();

	private string s_playerChamsColor = Settings.c_playerChams.GetHexCode();

	private string s_enemyChamsColor = Settings.c_enemyChams.GetHexCode();

	private string s_landmineChamsColor = Settings.c_landmineChams.GetHexCode();

	private string s_turretChamsColor = Settings.c_turretChams.GetHexCode();

	private string s_doorLockChamsColor = Settings.c_doorLockChams.GetHexCode();

	private string s_valveChamsColor = Settings.c_steamHazardChams.GetHexCode();

	private string s_bigDoorChamsColor = Settings.c_bigDoorChams.GetHexCode();

	private string s_shipChamsColor = Settings.c_shipChams.GetHexCode();

	private string s_breakerChamsColor = Settings.c_breakerChams.GetHexCode();

	private string s_spikeRoofTrapChamsColor = Settings.c_spikeRoofTrapChams.GetHexCode();

	private string s_mineshaftElevatorChamsColor = Settings.c_mineshaftElevatorChams.GetHexCode();

	private string s_enemyVentChamsColor = Settings.c_enemyVentChams.GetHexCode();

	private string s_vainShroudChamsColor = Settings.c_vainShroudChams.GetHexCode();

	private string s_itemDropShipChamsColor = Settings.c_itemDropShipChams.GetHexCode();

	private string s_cruiserChamsColor = Settings.c_CruiserChams.GetHexCode();

	private string s_causeOfDeath = Settings.c_causeOfDeath.GetHexCode();

	private string s_lootTierColors = string.Join(",", Array.ConvertAll(Settings.c_scrapValueColors, (RGBAColor x) => x.GetHexCode()));

	private string s_lootTiers = string.Join(",", Settings.i_scrapValueThresholds);

	private int i_selectedCrosshairIndex = -1;


	private int i_themeIndex = -1;

	private float f_leftWidth;

	public SettingsTab()
		: base("SettingsTab.Title")
	{
	}

	public override void Draw()
	{
		f_leftWidth = HackMenu.Instance.contentWidth * 0.55f - (float)HackMenu.Instance.spaceFromLeft;
		if (i_selectedCrosshairIndex == -1)
		{
			i_selectedCrosshairIndex = (int)Settings.ct_crosshairType;
		}
		if (i_themeIndex == -1)
		{
			i_themeIndex = Array.IndexOf(Theme.GetThemes(), Theme.name);
		}
		GUILayout.BeginVertical(GUILayout.Width(f_leftWidth));
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		MenuContent();
		ControlSettingsContent();
		VisualSettingsContent();
		ColorContent();
		ESPSettingsContent();
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.45f - (float)HackMenu.Instance.spaceFromLeft));
		KeybindContent();
		GUILayout.EndVertical();
	}

	// The remote status is changed by a background thread. IMGUI draws every frame twice (Layout, then Repaint)
	// and both passes must create the same controls, so read it once per frame, on the Layout pass.
	private string remoteStatusShown = "";

	private string remotePairCodeShown;

	private void RemoteContent()
	{
		if (Event.current.type == EventType.Layout)
		{
			remoteStatusShown = Remote.RemoteControl.StatusText;
			remotePairCodeShown = Remote.RemoteControl.PairCode;
		}
		UI.Header("SettingsTab.Remote", space: true);
		// One full-width line: as a two-column row the long status squeezed the "Status" label into "Statu s".
		// While waiting to be paired the code has its own line below, so the status stays short.
		string shownStatus = remotePairCodeShown != null ? "Waiting to be paired" : remoteStatusShown;
		GUILayout.Label(Localization.Localize("SettingsTab.RemoteStatus") + ": " + shownStatus);
		if (remotePairCodeShown != null)
		{
			UI.Label("SettingsTab.RemoteCode", remotePairCodeShown, Settings.c_hackhighlight);
			UI.Label("SettingsTab.RemoteHowTo");
		}
		bool enabled = Remote.RemoteControl.Config.Enabled;
		UI.Actions(new UIButton(enabled ? "SettingsTab.RemoteDisable" : "SettingsTab.RemoteEnable", () =>
		{
			Remote.RemoteControl.SetEnabled(!enabled);
		}), new UIButton("SettingsTab.RemoteReconnect", () =>
		{
			Remote.RemoteControl.Start();
		}), new UIButton("SettingsTab.RemoteRepair", () =>
		{
			Remote.RemoteControl.ResetPairing();
		}));
		UI.Actions(new UIButton("SettingsTab.RemoteOpenConfig", () =>
		{
			System.Diagnostics.Process.Start("notepad.exe", Remote.RemoteConfig.FullPath);
		}));
	}

	private void MenuContent()
	{
		UI.Actions(new UIButton("SettingsTab.ResetSettings", () =>
		{
			Settings.Config.RegenerateConfig();
		}), new UIButton("SettingsTab.SaveSettings", () =>
		{
			Settings.Config.SaveConfig();
		}), new UIButton("SettingsTab.ReloadSettings", () =>
		{
			Settings.Config.LoadConfig();
		}));
		UI.Actions(new UIButton("SettingsTab.OpenSettings", () =>
		{
			Settings.Config.OpenConfig();
		}), new UIButton("SettingsTab.Eject", () =>
		{
			Ejector.Eject();
		}));
		RemoteContent();
		UI.Header("SettingsTab.General");
		UI.Select("SettingsTab.Theme", ref i_themeIndex, (from x in Theme.GetThemes()
			select new UIOption(x, () =>
			{
				Theme.SetTheme(x);
			})).ToArray());
		UI.NumSelect("SettingsTab.FontSize", ref Settings.i_menuFontSize, 5, 30);
		UI.NumSelect("SettingsTab.ScreenFontSize", ref Settings.i_screenFontSize, 5, 30);
		UI.NumSelect("SettingsTab.SliderSize", ref Settings.i_sliderWidth, 50, 120);
		UI.NumSelect("SettingsTab.TextboxSize", ref Settings.i_textboxWidth, 50, 120);
		UI.Slider("SettingsTab.MenuAlpha", Settings.f_menuAlpha.ToString("0.00"), ref Settings.f_menuAlpha, 0.1f, 1f, 0.1f);
		UI.Button("SettingsTab.ResizeMenu", () =>
		{
			MenuUtil.BeginResizeMenu();
		}, "SettingsTab.Resize");
		UI.Button("SettingsTab.ResetMenu", () =>
		{
			HackMenu.Instance.ResetMenuSize();
		}, "General.Reset");
		UI.Toggle("SettingsTab.FPSCounter", ref Settings.b_FPSCounter, "General.Enable", "General.Disable");
		UI.Slider("SettingsTab.ObjectQueueDelay", Settings.f_ObjectQueueDelay.ToString("0.0"), ref Settings.f_ObjectQueueDelay, 0f, 1f, 0.1f);
		UI.Toggle("SettingsTab.HackHighlight", ref Settings.b_HackHighlight, "General.Enable", "General.Disable");
		// Turning this on closes this menu at once. It comes back from the website, or with Left Ctrl + Left Alt + Insert.
		UI.Toggle("SettingsTab.StealthMode", ref Settings.b_StealthMode, "General.Enable", "General.Disable");
		UI.Toggle("SettingsTab.DebugMode", ref Settings.DebugMode, "General.Enable", "General.Disable", HackMenu.Instance.ToggleDebugTab);
	}

	private void ControlSettingsContent()
	{
		UI.Header("SettingsTab.MouseControl");
		UI.Slider("SettingsTab.mouseSens", Settings.f_mouseSensitivity.ToString("0.00"), ref Settings.f_mouseSensitivity, 0.1f, 1f);
		UI.Slider("SettingsTab.movementSpeed", Settings.f_inputMovementSpeed.ToString("0"), ref Settings.f_inputMovementSpeed, 10f, 30f);
	}

	private void VisualSettingsContent()
	{
		UI.Header("SettingsTab.Visual");
		UI.Slider("SettingsTab.MaxESP", Settings.f_espDistance.ToString("0") + "m", ref Settings.f_espDistance, 0f, 10000f);
		UI.Slider("SettingsTab.MinCham", Settings.f_chamDistance.ToString("0") + "m", ref Settings.f_chamDistance, 0f, 100f);
		UI.Slider("SettingsTab.CrosshairScale", Settings.f_crosshairScale.ToString("0.00"), ref Settings.f_crosshairScale, 4f, 24f);
		UI.Slider("SettingsTab.CrosshairThickness", Settings.f_crosshairThickness.ToString("0.00"), ref Settings.f_crosshairThickness, 1f, 5f);
		UI.Select("SettingsTab.CrosshairType", ref i_selectedCrosshairIndex, new UIOption("X", () =>
		{
			Settings.ct_crosshairType = CrosshairType.X;
		}), new UIOption("+", () =>
		{
			Settings.ct_crosshairType = CrosshairType.Plus;
		}), new UIOption("*", () =>
		{
			Settings.ct_crosshairType = CrosshairType.Dot;
		}));
		UI.Slider("SettingsTab.BreadcrumbInterval", Settings.f_breadcrumbInterval.ToString(), ref Settings.f_breadcrumbInterval, 1f, 10f);
		UI.Slider("SettingsTab.NVIntensity", Settings.f_nvIntensity.ToString(), ref Settings.f_nvIntensity, Settings.f_defaultNightVisionIntensity, 10000f);
		UI.Slider("SettingsTab.NVRange", Settings.f_nvRange.ToString(), ref Settings.f_nvRange, Settings.f_defaultNightVisionRange, 10000f);
		UI.Checkbox("SettingsTab.DisableModels", ref Settings.b_disableSpectatorModels);
	}

	private void ESPSettingsContent()
	{
		UI.Header("SettingsTab.ESP", space: true);
		UI.SubHeader("SettingsTab.Chams");
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.Width(f_leftWidth * 0.465f));
		UI.Button("SettingsTab.ToggleAllChams", () =>
		{
			ToggleAllChams();
		});
		UI.Checkbox("SettingsTab.Objects", ref Settings.b_chamsObject);
		UI.Checkbox("SettingsTab.Enemies", ref Settings.b_chamsEnemy);
		UI.Checkbox("SettingsTab.Players", ref Settings.b_chamsPlayer);
		UI.Checkbox("SettingsTab.Landmines", ref Settings.b_chamsLandmine);
		UI.Checkbox("SettingsTab.Breaker", ref Settings.b_chamsBreaker);
		UI.Checkbox("SettingsTab.MineshaftElevator", ref Settings.b_chamsMineshaftElevator);
		UI.Checkbox("SettingsTab.EnemyVent", ref Settings.b_chamsEnemyVent);
		UI.Checkbox("SettingsTab.VainShroud", ref Settings.b_chamsVainShroud);
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(f_leftWidth * 0.465f));
		UI.Checkbox("SettingsTab.Turrets", ref Settings.b_chamsTurret);
		UI.Checkbox("SettingsTab.Ship", ref Settings.b_chamsShip);
		UI.Checkbox("SettingsTab.SteamValves", ref Settings.b_chamsSteamHazard);
		UI.Checkbox("SettingsTab.BigDoors", ref Settings.b_chamsBigDoor);
		UI.Checkbox("SettingsTab.LockedDoors", ref Settings.b_chamsDoorLock);
		UI.Checkbox("SettingsTab.SpikeRoofTrap", ref Settings.b_chamsSpikeRoofTrap);
		UI.Checkbox("SettingsTab.ItemDropShip", ref Settings.b_chamsItemDropship);
		UI.Checkbox("SettingsTab.Cruiser", ref Settings.b_chamsCruiser);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		UI.SubHeader("SettingsTab.EnemyTypes", space: true);
		List<EnemyAIType> list = Enum.GetValues(typeof(EnemyAIType)).Cast<EnemyAIType>().ToList();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.Width(f_leftWidth * 0.465f));
		for (int num = 0; num < list.Count / 2; num++)
		{
			EnemyAIType type = list[num];
			if (type != EnemyAIType.Unknown)
			{
				UI.Checkbox(Handler.EnemyAIExtensions.ShownName(type.ToString()), type.IsESPEnabled(), () =>
				{
					type.ToggleESP();
				});
			}
		}
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(f_leftWidth * 0.465f));
		for (int num2 = list.Count / 2; num2 < list.Count; num2++)
		{
			EnemyAIType type2 = list[num2];
			if (type2 != EnemyAIType.Unknown)
			{
				UI.Checkbox(Handler.EnemyAIExtensions.ShownName(type2.ToString()), type2.IsESPEnabled(), () =>
				{
					type2.ToggleESP();
				});
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void ColorContent()
	{
		UI.Header("SettingsTab.ColorsHeader");
		UI.TextboxAction("SettingsTab.MenuBG", ref s_bgColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_background, s_bgColor);
		}));
		UI.TextboxAction("SettingsTab.MenuText", ref s_menuText, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_menuText, s_menuText);
		}));
		UI.TextboxAction("SettingsTab.Primary", ref s_primaryColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_primary, s_primaryColor);
		}));
		UI.TextboxAction("SettingsTab.Crosshair", ref s_crosshairColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_crosshair, s_crosshairColor);
		}));
		UI.TextboxAction("SettingsTab.HackHighlight", ref s_hackhighlightColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_hackhighlight, s_hackhighlightColor);
		}));
		UI.Header("SettingsTab.ESPColors", space: true);
		UI.TextboxAction("SettingsTab.Objects", ref s_objectESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_objectESP, s_objectESPColor);
		}));
		UI.TextboxAction("SettingsTab.Enemies", ref s_enemyESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_enemyESP, s_enemyESPColor);
		}));
		UI.TextboxAction("SettingsTab.Players", ref s_playerESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_playerESP, s_playerESPColor);
		}));
		UI.TextboxAction("SettingsTab.Bodies", ref s_bodyESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_bodyESP, s_bodyESPColor);
		}));
		UI.TextboxAction("SettingsTab.EntExtDoors", ref s_doorESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_entranceExitESP, s_doorESPColor);
		}));
		UI.TextboxAction("SettingsTab.Landmines", ref s_landmineESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_landmineESP, s_landmineESPColor);
		}));
		UI.TextboxAction("SettingsTab.Turrets", ref s_turretESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_turretESP, s_turretESPColor);
		}));
		UI.TextboxAction("SettingsTab.Ship", ref s_shipESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_shipESP, s_shipESPColor);
		}));
		UI.TextboxAction("SettingsTab.SteamValves", ref s_valveESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_steamHazardESP, s_valveESPColor);
		}));
		UI.TextboxAction("SettingsTab.BigDoors", ref s_bigDoorESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_bigDoorESP, s_bigDoorESPColor);
		}));
		UI.TextboxAction("SettingsTab.LockedDoors", ref s_doorLockESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_doorLockESP, s_doorLockESPColor);
		}));
		UI.TextboxAction("SettingsTab.Breaker", ref s_doorLockESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_breakerESP, s_breakerESPColor);
		}));
		UI.TextboxAction("SettingsTab.SpikeRoofTrap", ref s_bigDoorESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_spikeRoofTrapESP, s_spikeRoofTrapESPColor);
		}));
		UI.TextboxAction("SettingsTab.MineshaftElevator", ref s_mineshaftElevatorESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_mineshaftElevatorESP, s_mineshaftElevatorESPColor);
		}));
		UI.TextboxAction("SettingsTab.EnemyVent", ref s_enemyVentESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_enemyVentESP, s_enemyVentESPColor);
		}));
		UI.TextboxAction("SettingsTab.VainShroud", ref s_vainShroudESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_vainShroudESP, s_vainShroudESPColor);
		}));
		UI.TextboxAction("SettingsTab.Cruiser", ref s_cruiserESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_CruiserESP, s_cruiserESPColor);
		}));
		UI.TextboxAction("SettingsTab.ItemDropShip", ref s_itemDropShipESPColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_itemDropShipESP, s_itemDropShipESPColor);
		}));
		UI.Header("SettingsTab.ChamColors", space: true);
		UI.Toggle("SettingsTab.UseSingleChamColor", ref Settings.b_UseSingleChamColor, "General.Enable", "General.Disable", (bool _) =>
		{
			SetColor(ref Settings.c_chams, s_chamsColor, chams: true);
		});
		UI.TextboxAction("SettingsTab.Chams", ref s_chamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_chams, s_chamsColor, chams: true);
		}));
		if (!Settings.b_UseSingleChamColor)
		{
			UI.TextboxAction("SettingsTab.ObjectsChams", ref s_objectChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_objectChams, s_objectChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.EnemiesChams", ref s_enemyChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_enemyChams, s_enemyChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.PlayersChams", ref s_playerChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_playerChams, s_playerChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.LandminesChams", ref s_landmineChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_landmineChams, s_landmineChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.TurretsChams", ref s_turretChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_turretChams, s_turretChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.ShipChams", ref s_shipChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_shipChams, s_shipChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.SteamValvesChams", ref s_valveChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_steamHazardChams, s_valveChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.BigDoorsChams", ref s_bigDoorChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_bigDoorChams, s_bigDoorChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.LockedDoorsChams", ref s_doorLockChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_doorLockChams, s_doorLockChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.BreakerChams", ref s_doorLockChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_breakerChams, s_breakerChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.SpikeRoofTrapChams", ref s_bigDoorChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_spikeRoofTrapChams, s_spikeRoofTrapChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.EnemyVentChams", ref s_enemyVentChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_enemyVentChams, s_enemyVentChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.MineshaftElevatorChams", ref s_mineshaftElevatorChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_mineshaftElevatorChams, s_mineshaftElevatorChamsColor);
			}));
			UI.TextboxAction("SettingsTab.VainShroudChams", ref s_vainShroudChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_vainShroudChams, s_vainShroudChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.CruiserChams", ref s_cruiserChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_CruiserChams, s_cruiserChamsColor, chams: true);
			}));
			UI.TextboxAction("SettingsTab.ItemDropShipChams", ref s_itemDropShipChamsColor, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
			{
				SetColor(ref Settings.c_itemDropShipChams, s_itemDropShipChamsColor, chams: true);
			}));
		}
		UI.Header("SettingsTab.TieredLootHeader", space: true);
		if (s_tierColorError != "")
		{
			UI.Label(s_tierColorError, Settings.c_error);
		}
		UI.Button(new string[2]
		{
			"SettingsTab.TieredLoot",
			"(" + GetTiersColored() + ")"
		}, () =>
		{
			EditTierColors();
		}, "General.Set");
		UI.Textbox("SettingsTab.Tiers", ref s_lootTiers, "[^0-9,]");
		UI.Textbox("SettingsTab.Colors", ref s_lootTierColors, "[^0-9A-Za-z,]");
		UI.Header("SettingsTab.OtherColors", space: true);
		UI.TextboxAction("SettingsTab.CauseOfDeath", ref s_causeOfDeath, "[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
		{
			SetColor(ref Settings.c_causeOfDeath, s_causeOfDeath);
		}));
	}

	private void KeybindContent()
	{
		UI.Header("SettingsTab.Keybinds");
		if (s_kbError != "")
		{
			UI.Label(s_kbError, Settings.c_error);
		}
		GUILayout.BeginVertical();
		scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
		UI.Textbox("General.Search", ref s_kbSearch, "", 3, big: false);
		if (s_kbSearch.Length > 0 && GUILayout.Button("Clear search"))
		{
			s_kbSearch = "";
			GUIUtility.keyboardControl = 0;
		}
		List<Hack> list = Enum.GetValues(typeof(Hack)).Cast<Hack>().ToList()
			.FindAll((Hack x) => x.ToString().ToLower().Contains(s_kbSearch.ToLower()));
		foreach (Hack item in list)
		{
			if (item.CanHaveKeyBind())
			{
				GUILayout.BeginHorizontal();
				ButtonControl keyBind = item.GetKeyBind();
				string text;
				if (item.HasKeyBind())
				{
					text = ((keyBind is KeyControl { keyCode: var keyCode }) ? keyCode.ToString() : keyBind.displayName);
				}
				else
				{
					text = "None";
				}
				GUILayout.Label(item.ToString());
				GUILayout.FlexibleSpace();
				if (item.HasKeyBind() && item != Hack.OpenMenu && GUILayout.Button("-"))
				{
					item.RemoveKeyBind();
				}
				string text2 = (item.IsWaiting() ? "Waiting" : text);
				if (GUILayout.Button(text2, GUILayout.Width(85f)))
				{
					KBUtil.BeginChangeKeyBind(item);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void SetColor(ref RGBAColor color, string hexCode, bool chams = false)
	{
		while (hexCode.Length < 6)
		{
			hexCode += "0";
		}
		color = new RGBAColor(hexCode);
		if (chams)
		{
			ChamHandler.RefreshChams();
		}
		Settings.Config.SaveConfig();
	}

	private void EditTierColors()
	{
		int[] array = Array.ConvertAll(s_lootTiers.Split(','), (string x) => int.TryParse(x, out var result) ? result : 0);
		RGBAColor[] array2 = Array.ConvertAll(s_lootTierColors.Split(','), (string x) => new RGBAColor(x));
		if (array.Length != array2.Length)
		{
			s_tierColorError = "SettingsTab.TierColorError";
			return;
		}
		Settings.i_scrapValueThresholds = array;
		Settings.c_scrapValueColors = array2;
		Settings.Config.SaveConfig();
	}

	private string GetTiersColored()
	{
		string[] array = new string[Settings.i_scrapValueThresholds.Length];
		for (int i = 0; i < Settings.i_scrapValueThresholds.Length; i++)
		{
			int num = Settings.i_scrapValueThresholds[i];
			RGBAColor rGBAColor = Settings.c_scrapValueColors[i];
			array[i] = rGBAColor.AsString(num.ToString());
		}
		return string.Join(",", array);
	}

	public static void ToggleAllChams()
	{
		Settings.b_chamsObject = !Settings.b_chamsObject;
		Settings.b_chamsEnemy = !Settings.b_chamsEnemy;
		Settings.b_chamsPlayer = !Settings.b_chamsPlayer;
		Settings.b_chamsLandmine = !Settings.b_chamsLandmine;
		Settings.b_chamsTurret = !Settings.b_chamsTurret;
		Settings.b_chamsBigDoor = !Settings.b_chamsBigDoor;
		Settings.b_chamsDoorLock = !Settings.b_chamsDoorLock;
		Settings.b_chamsSteamHazard = !Settings.b_chamsSteamHazard;
		Settings.b_chamsBreaker = !Settings.b_chamsBreaker;
		Settings.b_chamsShip = !Settings.b_chamsShip;
		Settings.b_chamsSpikeRoofTrap = !Settings.b_chamsSpikeRoofTrap;
		Settings.b_UseSingleChamColor = !Settings.b_UseSingleChamColor;
		Settings.b_chamsMineshaftElevator = !Settings.b_chamsMineshaftElevator;
		Settings.b_chamsEnemyVent = !Settings.b_chamsEnemyVent;
		Settings.b_chamsItemDropship = !Settings.b_chamsItemDropship;
		Settings.b_chamsCruiser = !Settings.b_chamsCruiser;
		Settings.b_chamsVainShroud = !Settings.b_chamsVainShroud;
	}
}
