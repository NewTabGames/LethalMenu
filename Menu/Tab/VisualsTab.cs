using LethalMenu.Cheats;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

internal class VisualsTab : MenuTab
{
	private Vector2 scrollPos = Vector2.zero;

	private Vector2 scrollPos2 = Vector2.zero;

	public VisualsTab()
		: base("VisualsTab.Title")
	{
	}

	public override void Draw()
	{
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - (float)HackMenu.Instance.spaceFromLeft));
		ESPMenuContent();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - (float)HackMenu.Instance.spaceFromLeft));
		OtherVisualsContent();
		GUILayout.EndVertical();
	}

	private void ESPMenuContent()
	{
		UI.Header("ESP");
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		UI.SubHeader("Style (labels show unless only Box ESP is on)");
		UI.Hack(Hack.ToggleAllESP, "VisualsTab.ToggleAll");
		UI.Hack(Hack.NameESP, "VisualsTab.NameESP");
		UI.HackSlider(Hack.BoxESP, "VisualsTab.BoxESP", Settings.f_ESPThickness.ToString("0"), ref Settings.f_ESPThickness, 1f, 10f);
		UI.Toggle("VisualsTab.UseScrapTiers", ref Settings.b_useScrapTiers, "General.Enable", "General.Disable");
		UI.Toggle("VisualsTab.VCDisplay", ref Settings.b_VCDisplay, "General.Enable", "General.Disable");
		UI.Toggle("VisualsTab.PlayerHPDisplay", ref Settings.b_PlayerHPDisplay, "General.Enable", "General.Disable");
		UI.SubHeader("People & Creatures", space: true);
		UI.Hack(Hack.PlayerESP, "VisualsTab.PlayerESP");
		UI.Hack(Hack.BodyESP, "VisualsTab.BodyESP");
		UI.Hack(Hack.EnemyESP, "VisualsTab.EnemyESP");
		UI.Hack(Hack.EnemyVentESP, "VisualsTab.EnemyVentESP");
		UI.SubHeader("Loot", space: true);
		UI.Hack(Hack.ObjectESP, "VisualsTab.ObjectESP");
		UI.Hack(Hack.ItemDropShipESP, "VisualsTab.ItemDropShipESP");
		UI.SubHeader("Hazards", space: true);
		UI.Hack(Hack.LandmineESP, "VisualsTab.LandmineESP");
		UI.Hack(Hack.TurretESP, "VisualsTab.TurretESP");
		UI.Hack(Hack.SpikeRoofTrapESP, "VisualsTab.SpikeRoofTrapESP");
		UI.Hack(Hack.SteamHazardESP, "VisualsTab.SteamHazardESP");
		UI.Hack(Hack.VainShroudESP, "VisualsTab.VainShroudESP");
		UI.SubHeader("Places", space: true);
		UI.Hack(Hack.ShipESP, "VisualsTab.ShipESP");
		UI.Hack(Hack.DoorESP, "VisualsTab.EntExtDoorsESP");
		UI.Hack(Hack.BigDoorESP, "VisualsTab.BigDoorESP");
		UI.Hack(Hack.DoorLockESP, "VisualsTab.LockedDoorESP");
		UI.Hack(Hack.BreakerESP, "VisualsTab.BreakerESP");
		UI.Hack(Hack.MineshaftElevatorESP, "VisualsTab.MineshaftElevatorESP");
		UI.Hack(Hack.CruiserESP, "VisualsTab.CruiserESP");
		GUILayout.EndScrollView();
	}

	private void OtherVisualsContent()
	{
		UI.Header("VisualsTab.OtherVisuals");
		scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
		UI.SubHeader("HUD");
		UI.Hack(Hack.AlwaysShowClock, "VisualsTab.ShowClock");
		UI.Hack(Hack.SimpleClock, "VisualsTab.SimpleClock");
		UI.Hack(Hack.Crosshair, "VisualsTab.Crosshair");
		UI.Hack(Hack.HPDisplay, "VisualsTab.HPDisplay");
		UI.Hack(Hack.Breadcrumbs, "VisualsTab.Breadcrumbs");
		UI.SubHeader("Camera & Screen", space: true);
		UI.HackSlider(Hack.FOV, "VisualsTab.FOV", Settings.f_fov.ToString("0"), ref Settings.f_fov, 10f, 180f, 1f);
		UI.Hack(Hack.NightVision, "SelfTab.NightVision");
		UI.ToggleAction("VisualsTab.NoFog", ref Settings.b_NoFog, "General.Enable", "General.Disable", NoFog.ToggleNoFog);
		UI.Hack(Hack.NoVisor, "VisualsTab.NoVisor");
		UI.Hack(Hack.NoFieldOfDepth, "VisualsTab.NoFieldOfDepth");
		UI.Hack(Hack.NoFlash, "SelfTab.NoFlash");
		UI.Hack(Hack.NoCameraShake, "SelfTab.NoCameraShake");
		UI.Hack(Hack.FullRenderResolution, "SelfTab.FullRenderResolution");
		GUILayout.EndScrollView();
	}
}
