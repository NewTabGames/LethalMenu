using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

internal class TrollTab : MenuTab
{
	private Vector2 scrollPos = Vector2.zero;

	public TrollTab()
		: base("TrollTab.Title")
	{
	}

	public override void Draw()
	{
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - (float)HackMenu.Instance.spaceFromLeft));
		TrollMenuContent();
		GUILayout.EndVertical();
	}

	private void TrollMenuContent()
	{
		UI.Header("TrollTab.Title");
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		UI.SubHeader("Sounds");
		UI.Hack(Hack.ToggleShipHorn, "TrollTab.ShipHorn");
		UI.Hack(Hack.ToggleCarHorn, "TrollTab.CarHorn");
		UI.Hack(Hack.ToggleTerminalSound, "TrollTab.TerminalSound");
		UI.Hack(Hack.ToggleDepositDeskDoorSound, "TrollTab.ToggleDepositDeskDoorSound");
		UI.HackSlider(Hack.PJSpammer, "TrollTab.PJSpammer", Settings.f_pjSpamSpeed.ToString("0.00"), ref Settings.f_pjSpamSpeed, 0f, 1f);
		UI.Hack(Hack.SlideTaunt, "TrollTab.SlideTaunt");
		UI.SubHeader("Ship & Facility", space: true);
		UI.Hack(Hack.ToggleShipLights, "TrollTab.ShipLights");
		UI.Hack(Hack.ToggleFactoryLights, new string[2] { "TrollTab.FactoryLights", "General.HostTag" });
		UI.Hack(Hack.OpenShipDoorSpace, "TrollTab.OpenShipDoorSpace");
		UI.Hack(Hack.EjectEveryone, new string[2] { "TrollTab.EjectEveryone", "General.HostTag" });
		UI.Hack(Hack.ToggleMineshaftElevator, "TrollTab.ToggleMineshaftElevator");
		UI.SubHeader("Traps & Explosives", space: true);
		UI.Hack(Hack.BlowUpAllLandmines, "TrollTab.BlowAllMines");
		UI.Hack(Hack.ToggleAllLandmines, "TrollTab.ToggleMines");
		UI.Hack(Hack.ToggleAllTurrets, "TrollTab.ToggleTurrets");
		UI.Hack(Hack.BerserkAllTurrets, "TrollTab.BerserkAllTurrets");
		UI.Hack(Hack.ForceBridgeFall, "TrollTab.ForceBridgeFall");
		UI.Hack(Hack.ForceSmallBridgeFall, "TrollTab.ForceSmallBridgeFall");
		UI.Hack(Hack.ExplodeCruiser, "TrollTab.ExplodeCruiser");
		UI.Hack(Hack.ExplodeAllJetpacks, "TrollTab.ExplodeAllJetpacks");
		UI.Hack(Hack.ExplodeJetpacksOnGrab, "TrollTab.ExplodeJetpacksOnGrab");
		UI.Hack(Hack.ToggleVehicleMagnet, "TrollTab.ToggleVehicleMagnet");
		UI.SubHeader("Shotguns", space: true);
		UI.Hack(Hack.ShootAllShotguns, "TrollTab.ShootAllShotguns");
		UI.Hack(Hack.SpamShootAllShotguns, "TrollTab.SpamShootAllShotguns");
		UI.SubHeader("Monsters", space: true);
		UI.Hack(Hack.ForceTentacleAttack, "TrollTab.TentacleAttack");
		UI.Hack(Hack.SpawnMaskedEnemy, "TrollTab.SpawnMasks");
		UI.Hack(Hack.SpawnHoardingBugInfestation, new string[2] { "TrollTab.SpawnHoardingBugInfestation", "General.HostTag" });
		GUILayout.EndScrollView();
	}
}
