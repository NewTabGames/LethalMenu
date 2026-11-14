using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using LethalMenu.Language;
using LethalMenu.Types;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class ESP : Cheat
{
	public ESP()
	{
		ChamHandler.SetupChamMaterial();
	}

	public override void OnGui()
	{
		if (!StartOfRound.Instance)
		{
			return;
		}
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		if (localPlayerController == null)
		{
			return;
		}
		Run(Hack.ObjectESP, DisplayScrap);
		Run(Hack.EnemyESP, DisplayEnemyAI);
		Run(Hack.PlayerESP, DisplayPlayers);
		Run(Hack.BodyESP, DisplayBodies);
		Run(Hack.DoorESP, DisplayEntranceExitDoors);
		Run(Hack.LandmineESP, DisplayLandmines);
		Run(Hack.TurretESP, DisplayTurrets);
		Run(Hack.ShipESP, DisplayShip);
		Run(Hack.BigDoorESP, DisplayBigDoors);
		Run(Hack.SteamHazardESP, DisplaySteamHazards);
		Run(Hack.DoorLockESP, DisplayDoorLocks);
		Run(Hack.BreakerESP, DisplayBreaker);
		Run(Hack.SpikeRoofTrapESP, DisplaySpikeRoofTraps);
		Run(Hack.MineshaftElevatorESP, DisplayElevator);
		Run(Hack.EnemyVentESP, DisplayEnemyVents);
		Run(Hack.VainShroudESP, DisplayVainShrouds);
		Run(Hack.CruiserESP, DisplayCruiser);
		Run(Hack.ItemDropShipESP, DisplayItemDropShip);
	}

	/// <summary>Labels are drawn unless only Box ESP is chosen, so turning on an ESP type always shows something.</summary>
	private static bool ShowNames => Hack.NameESP.IsEnabled() || !Hack.BoxESP.IsEnabled();

	private static bool ShowBoxes => Hack.BoxESP.IsEnabled();

	private static void Run(Hack hack, Action draw)
	{
		if (!hack.IsEnabled())
		{
			return;
		}
		try
		{
			draw();
		}
		catch (Exception ex)
		{
			Settings.debugMessage = hack + " ESP: " + ex.Message + "\n" + ex.StackTrace;
		}
	}

	private static string ObjectCode(Component component)
	{
		TerminalAccessibleObject terminalObject = component.GetComponent<TerminalAccessibleObject>();
		if (terminalObject == null)
		{
			terminalObject = component.GetComponentInParent<TerminalAccessibleObject>();
		}
		if (terminalObject == null)
		{
			terminalObject = component.GetComponentInChildren<TerminalAccessibleObject>();
		}
		return (terminalObject != null) ? terminalObject.objectCode : "";
	}

	public override void Update()
	{
		DisplayChams();
	}

	private void DisplayChams<T>(IEnumerable<T> objects, Func<T, RGBAColor> colorSelector) where T : UnityEngine.Object
	{
		if (objects == null)
		{
			return;
		}
		foreach (T item in objects?.Where((T o) => o != null))
		{
			if (1 == 0)
			{
			}
			Transform transform2;
			if (item is Transform transform)
			{
				transform2 = transform;
			}
			else if (item is Component component)
			{
				transform2 = component.transform;
			}
			else
			{
				transform2 = ((!(item is GameObject gameObject)) ? null : gameObject.transform);
			}
			if (1 == 0)
			{
			}
			Transform transform3 = transform2;
			if (!(transform3 == null))
			{
				float distanceToPlayer = GetDistanceToPlayer(transform3.position);
				if (distanceToPlayer != 0f)
				{
					item.GetChamHandler()?.ProcessCham(distanceToPlayer);
				}
			}
		}
	}

	public void DisplayChams()
	{
		DisplayChams(LethalMenu.items?.Where((GrabbableObject i) => i != null), (GrabbableObject _) => Settings.c_chams);
		DisplayChams(LethalMenu.landmines, (Landmine _) => Settings.c_chams);
		DisplayChams(LethalMenu.turrets?.Where((Turret t) => t != null && t.gameObject != null && t.gameObject.transform?.parent?.gameObject != null).Select((Turret t) => t.gameObject.transform.parent.gameObject), (GameObject _) => Settings.c_chams);
		DisplayChams(LethalMenu.spikeRoofTraps?.Where((SpikeRoofTrap s) => s != null && (bool)s.gameObject && s.gameObject.transform?.parent?.gameObject != null).Select((SpikeRoofTrap s) => s?.gameObject.transform?.parent?.gameObject), (GameObject _) => Settings.c_chams);
		DisplayChams(LethalMenu.players?.Where((PlayerControllerB p) => p != null && p.actualClientId != LethalMenu.localPlayer?.actualClientId), (PlayerControllerB _) => Settings.c_chams);
		DisplayChams(LethalMenu.enemies?.Where((EnemyAI e) => e != null), (EnemyAI _) => Settings.c_chams);
		DisplayChams(LethalMenu.steamValves?.Where((SteamValveHazard s) => s != null), (SteamValveHazard _) => Settings.c_chams);
		DisplayChams(LethalMenu.bigDoors?.Where((TerminalAccessibleObject b) => b != null), (TerminalAccessibleObject _) => Settings.c_chams);
		DisplayChams(LethalMenu.doorLocks?.Where((DoorLock d) => d != null), (DoorLock _) => Settings.c_chams);
		DisplayChams(LethalMenu.enemyVents?.Where((EnemyVent e) => e != null), (EnemyVent _) => Settings.c_chams);
		DisplayChams(LethalMenu.vainShrouds?.Where((GameObject v) => v != null)?.Where((GameObject v) => v != null), (GameObject _) => Settings.c_chams);
		DisplayChams(LethalMenu.vehicles?.Where((VehicleController v) => v != null), (VehicleController _) => Settings.c_chams);
		DisplayChams(new ItemDropship[1] { LethalMenu.itemDropship }, (ItemDropship _) => Settings.c_chams);
		DisplayChams(new HangarShipDoor[1] { LethalMenu.shipDoor }, (HangarShipDoor _) => Settings.c_chams);
		DisplayChams(new BreakerBox[1] { LethalMenu.breaker }, (BreakerBox _) => Settings.c_chams);
		DisplayChams(new MineshaftElevatorController[1] { LethalMenu.mineshaftElevator }, (MineshaftElevatorController _) => Settings.c_chams);
	}

	private void DisplayObjects<T>(IEnumerable<T> objects, Func<T, string> labelSelector, Func<T, RGBAColor> colorSelector) where T : Component
	{
		if (objects == null)
		{
			return;
		}
		foreach (T item in objects?.Where((T o) => o != null && o.gameObject.activeSelf))
		{
			float distanceToPlayer = GetDistanceToPlayer(item.transform.position);
			if (distanceToPlayer != 0f && !(distanceToPlayer > Settings.f_espDistance) && Cheat.WorldToScreen(item.transform.position, out var screen))
			{
				if (ShowNames)
				{
					VisualUtil.DrawDistanceString(screen, labelSelector(item), colorSelector(item), distanceToPlayer);
				}
				if (ShowBoxes)
				{
					VisualUtil.DrawBoxOutline(item.gameObject, colorSelector(item), Settings.f_ESPThickness);
				}
			}
		}
	}

	private string Format(string label, params object[] args)
	{
		return string.Format(Localization.Localize(label), args);
	}

	private void DisplayTurrets()
	{
		DisplayObjects(LethalMenu.turrets?.Where((Turret t) => t != null && t.IsSpawned), (Turret turret) => Format("Cheats.ESP.Turret", ObjectCode(turret)), (Turret turret) => Settings.c_turretESP);
	}

	/// <summary>
	/// Uses the ship itself (StartOfRound's door node / ship bounds) instead of the tracked HangarShipDoor,
	/// which was often null or sitting where the label could not be seen.
	/// </summary>
	private void DisplayShip()
	{
		StartOfRound round = StartOfRound.Instance;
		Transform anchor = null;
		if (round.shipDoorNode != null)
		{
			anchor = round.shipDoorNode;
		}
		else if (round.middleOfShipNode != null)
		{
			anchor = round.middleOfShipNode;
		}
		else if (round.elevatorTransform != null)
		{
			anchor = round.elevatorTransform;
		}
		else if (LethalMenu.shipDoor != null)
		{
			anchor = LethalMenu.shipDoor.transform;
		}
		if (anchor == null)
		{
			return;
		}
		Vector3 position = anchor.position + Vector3.up * 1.5f;
		float distance = GetDistanceToPlayer(position);
		if (distance > Settings.f_espDistance)
		{
			return;
		}
		if (ShowNames && Cheat.WorldToScreen(position, out var screen))
		{
			VisualUtil.DrawDistanceString(screen, Format("Cheats.ESP.Ship"), Settings.c_shipESP, distance);
		}
		if (ShowBoxes && round.shipBounds != null)
		{
			VisualUtil.DrawBoxOutline(round.shipBounds.bounds, Settings.c_shipESP.GetColor(), Settings.f_ESPThickness);
		}
	}

	private void DisplayElevator()
	{
		DisplayObjects(new MineshaftElevatorController[1] { LethalMenu.mineshaftElevator }, (MineshaftElevatorController elevator) => Format("Cheats.ESP.MineshaftElevator"), (MineshaftElevatorController elevator) => Settings.c_mineshaftElevatorESP);
	}

	private void DisplayBreaker()
	{
		DisplayObjects(new BreakerBox[1] { LethalMenu.breaker }, (BreakerBox breaker) => Format("Cheats.ESP.BreakerBox"), (BreakerBox breaker) => Settings.c_breakerESP);
	}

	private void DisplayEntranceExitDoors()
	{
		DisplayObjects(LethalMenu.doors, (EntranceTeleport door) => door.isEntranceToBuilding ? Format("Cheats.ESP.Entrance") : Format("Cheats.ESP.Exit"), (EntranceTeleport door) => Settings.c_entranceExitESP);
	}

	private void DisplayLandmines()
	{
		DisplayObjects(LethalMenu.landmines?.Where((Landmine m) => m != null && m.IsSpawned && !m.hasExploded), (Landmine mine) => Format("Cheats.ESP.Landmine", ObjectCode(mine)), (Landmine mine) => Settings.c_landmineESP);
	}

	private void DisplayPlayers()
	{
		DisplayObjects(LethalMenu.players?.Where((PlayerControllerB p) => p != null && !p.isPlayerDead && p.IsRealPlayer() && p != LethalMenu.localPlayer), (PlayerControllerB player) => ((Settings.b_VCDisplay && player.voicePlayerState != null && player.voicePlayerState.IsSpeaking) ? "[VC] " : "") + (Settings.b_PlayerHPDisplay ? $"[HP: {player.health}] " : "") + (player.playerUsername ?? "Unknown"), (PlayerControllerB player) => Settings.c_playerESP);
	}

	private void DisplayEnemyAI()
	{
		DisplayObjects(LethalMenu.enemies?.Where((EnemyAI e) => e != null && !e.isEnemyDead && e.GetEnemyAIType().IsESPEnabled()), (EnemyAI enemy) => enemy.ShownName(), (EnemyAI enemy) => Settings.c_enemyESP);
	}

	private void DisplayScrap()
	{
		DisplayObjects(LethalMenu.items?.Where((GrabbableObject i) => i != null && !i.heldByPlayerOnServer && !i.isHeld && !i.isPocketed && i.IsSpawned && i.itemProperties != null && !i.deactivated && !(i is RagdollGrabbableObject)), (GrabbableObject item) =>
		{
			if (item is GiftBoxItem obj)
			{
				Item value = obj.Reflect().GetValue<Item>("objectInPresentItem");
				if ((object)value != null)
				{
					int value2 = obj.Reflect().GetValue<int>("objectInPresentValue");
					if (true)
					{
						return $"{item.itemProperties.itemName} ( {item.scrapValue} ) - {value.itemName} ( {value2} )";
					}
				}
			}
			return $"{item.itemProperties.itemName} ( {item.scrapValue} )";
		}, (GrabbableObject item) =>
		{
			if (!Settings.b_useScrapTiers)
			{
				return Settings.c_objectESP;
			}
			int num = Array.FindLastIndex(Settings.i_scrapValueThresholds, (int x) => x <= item.scrapValue);
			return (num > -1) ? Settings.c_scrapValueColors[num] : Settings.c_objectESP;
		});
	}

	private void DisplayBodies()
	{
		DisplayObjects(LethalMenu.items?.Where((GrabbableObject i) => i != null && !i.heldByPlayerOnServer && !i.isHeld && !i.isPocketed && i.IsSpawned && i is RagdollGrabbableObject), (GrabbableObject ragdoll) => (ragdoll is RagdollGrabbableObject ragdollGrabbableObject) ? (StartOfRound.Instance.allPlayerScripts[ragdollGrabbableObject.ragdoll.playerObjectId].playerUsername + " - " + Settings.c_causeOfDeath.AsString(ragdollGrabbableObject.ragdoll.causeOfDeath.ToString())) : null, (GrabbableObject ragdoll) => Settings.c_deadPlayer);
	}

	private void DisplaySteamHazards()
	{
		DisplayObjects(LethalMenu.steamValves?.Where((SteamValveHazard v) => v != null && !v.Reflect().GetValue<bool>("valveHasBeenRepaired")), (SteamValveHazard valve) => Format("Cheats.ESP.SteamValve"), (SteamValveHazard valve) => Settings.c_steamHazardESP);
	}

	private void DisplayBigDoors()
	{
		DisplayObjects(LethalMenu.bigDoors?.Where((TerminalAccessibleObject d) => d != null && d.isBigDoor), (TerminalAccessibleObject door) => Format("Cheats.ESP.BigDoors", door.objectCode), (TerminalAccessibleObject door) => Settings.c_bigDoorESP);
	}

	private void DisplayDoorLocks()
	{
		DisplayObjects(LethalMenu.doorLocks?.Where((DoorLock d) => d != null && d.isLocked), (DoorLock door) => Format("Cheats.ESP.LockedDoor"), (DoorLock door) => Settings.c_doorLockESP);
	}

	private void DisplaySpikeRoofTraps()
	{
		DisplayObjects(LethalMenu.spikeRoofTraps?.Where((SpikeRoofTrap t) => t != null && t.trapActive), (SpikeRoofTrap trap) => Format("Cheats.ESP.SpikeRoofTrap"), (SpikeRoofTrap trap) => Settings.c_spikeRoofTrapESP);
	}

	private void DisplayEnemyVents()
	{
		DisplayObjects(LethalMenu.enemyVents?.Where((EnemyVent e) => e != null && !e.ventIsOpen), (EnemyVent vent) => Format("Cheats.ESP.EnemyVent"), (EnemyVent vent) => Settings.c_enemyVentESP);
	}

	private void DisplayVainShrouds()
	{
		DisplayObjects(LethalMenu.vainShrouds?.Where((GameObject v) => v != null && v.transform != null).Select((GameObject v) => v.transform) ?? Enumerable.Empty<Transform>(), (Transform vain) => Format("Cheats.ESP.VainShroud"), (Transform vain) => Settings.c_vainShroudESP);
	}

	private void DisplayItemDropShip()
	{
		DisplayObjects(new ItemDropship[1] { LethalMenu.itemDropship }.Where((ItemDropship d) => d != null && d.deliveringOrder).ToList(), (ItemDropship dropship) => Format("Cheats.ESP.ItemDropShip"), (ItemDropship dropship) => Settings.c_itemDropShipESP);
	}

	private void DisplayCruiser()
	{
		DisplayObjects(LethalMenu.vehicles?.Where((VehicleController v) => v != null), (VehicleController vehicle) => Format("Cheats.ESP.Cruiser"), (VehicleController vehicle) => Settings.c_CruiserESP);
	}
}
