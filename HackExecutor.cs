using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Handler;
using LethalMenu.Manager;
using LethalMenu.Types;
using LethalMenu.Util;
using Steamworks;
using UnityEngine;

namespace LethalMenu;

public class HackExecutor
{
	public static void ToggleAllESP()
	{
		Hack.ObjectESP.Execute();
		Hack.EnemyESP.Execute();
		Hack.PlayerESP.Execute();
		Hack.BodyESP.Execute();
		Hack.DoorESP.Execute();
		Hack.LandmineESP.Execute();
		Hack.TurretESP.Execute();
		Hack.ShipESP.Execute();
		Hack.SteamHazardESP.Execute();
		Hack.BigDoorESP.Execute();
		Hack.DoorLockESP.Execute();
		Hack.BreakerESP.Execute();
		Hack.SpikeRoofTrapESP.Execute();
		Hack.MineshaftElevatorESP.Execute();
		Hack.EnemyVentESP.Execute();
		Hack.VainShroudESP.Execute();
		Hack.CruiserESP.Execute();
		Hack.ItemDropShipESP.Execute();
	}

	public static void ToggleAllDisplays()
	{
		Hack.DisplayBodies.Execute();
		Hack.DisplayDeadline.Execute();
		Hack.DisplayEnemies.Execute();
		Hack.DisplayMapObjects.Execute();
		Hack.DisplayQuota.Execute();
		Hack.DisplayShipObjects.Execute();
		Hack.DisplayCredits.Execute();
	}

	public static void ModExperience(int amt, ActionType type)
	{
		int localPlayerXP = amt;
		if (type == ActionType.Add)
		{
			localPlayerXP = HUDManager.Instance.localPlayerXP + amt;
		}
		if (type == ActionType.Remove)
		{
			localPlayerXP = HUDManager.Instance.localPlayerXP - amt;
		}
		HUDManager.Instance.localPlayerXP = localPlayerXP;
	}

	public static void ToggleCursor()
	{
		if ((bool)StartOfRound.Instance)
		{
			if (Cursor.visible)
			{
				MenuUtil.HideCursor();
			}
			else
			{
				MenuUtil.ShowCursor();
			}
		}
	}

	public static void OpenMenu()
	{
		if (Hack.OpenMenu.IsEnabled())
		{
			MenuUtil.ShowCursor();
		}
		else
		{
			MenuUtil.HideCursor();
		}
	}

	public static void ClickKill()
	{
		RoundHandler.ClickKill();
	}

	public static void ClickTeleport()
	{
		RoundHandler.ClickTeleport();
	}

	public static void UnlockObjects()
	{
		RoundHandler.UnlockObjects();
	}

	public static void UnloadMenu()
	{
		LethalMenu.Instance.Unload();
	}

	public static void NotifyDeath(PlayerControllerB died, CauseOfDeath cause)
	{
		HUDManager.Instance.DisplayTip("Lethal Menu", died.playerUsername + " has died from " + cause);
	}

	public static void NotifyEnemyDeath(EnemyType enemy)
	{
		HUDManager.Instance.DisplayTip("Lethal Menu", enemy.name + " has died");
	}

	public static void SpawnEnemy(EnemyType type, int num, bool outside)
	{
		RoundHandler.SpawnEnemy(type, num, outside);
	}

	public static void SpawnMaskedEnemy()
	{
		RoundHandler.SpawnMimicFromMasks();
	}

	public static void ToggleAllLandmines()
	{
		RoundHandler.ToggleAllLandmines();
	}

	public static void ToggleAllTurrets()
	{
		RoundHandler.ToggleAllTurrets();
	}

	public static void BlowUpAllLandmines()
	{
		RoundHandler.BlowUpAllLandmines();
	}

	public static void FixAllValves()
	{
		RoundHandler.FixAllValves();
	}

	public static void ToggleShipHorn()
	{
		RoundHandler.ToggleShipHorn();
	}

	public static void ForceBridgeFall()
	{
		RoundHandler.ForceBridgeFall();
	}

	public static void ForceSmallBridgeFall()
	{
		RoundHandler.ForceSmallBridgeFall();
	}

	public static void ForceTentacleAttack()
	{
		RoundHandler.ForceTentacleAttack();
	}

	public static void ModScrap(int value, int type)
	{
		RoundHandler.ModScrap(value, type);
	}

	public static void SpawnMoreScrap()
	{
		RoundHandler.SpawnScrap();
	}

	public static void ResetShip()
	{
		RoundHandler.ResetShip();
	}

	public static void EndGame()
	{
		RoundHandler.EndGame();
	}

	public static void StartGame()
	{
		RoundHandler.StartGame();
	}

	public static void ModQuota(int amt)
	{
		RoundHandler.SetQuota(amt);
	}

	public static void ModDeadline(int amt)
	{
		RoundHandler.SetDeadline(amt);
	}

	public static void Message(string msg, int type, int id)
	{
		RoundHandler.Message(msg, type, id);
	}

	public static void ModCredits(int amt, ActionType type)
	{
		RoundHandler.ModCredits(amt, type);
	}

	public static void BreakAllWebs()
	{
		RoundHandler.BreakAllWebs();
	}

	public static void SpawnLandmine()
	{
		RoundHandler.SpawnMapObject(MapObject.Landmine);
	}

	public static void SpawnTurret()
	{
		RoundHandler.SpawnMapObject(MapObject.TurretContainer);
	}

	public static void SpawnSpikeRoofTrap()
	{
		RoundHandler.SpawnMapObject(MapObject.SpikeRoofTrapHazard);
	}

	public static void SpawnMapObjects(MapObject type)
	{
		RoundHandler.SpawnMapObjects(type);
	}

	public static void SellQuota()
	{
		LethalMenu.localPlayer.Handle().SellQuota();
	}

	public static void SellEverything()
	{
		LethalMenu.localPlayer.Handle().PlaceEverythingOnDesk();
	}

	public static void ExplodeClosestMine(PlayerControllerB player)
	{
		player.Handle().ExplodeClosestLandmine();
	}

	public static void LureAllEnemies(PlayerControllerB player)
	{
		player.Handle().LureAllEnemies();
	}

	public static void SpiderWebPlayer(PlayerControllerB player)
	{
		player.Handle().SpawnSpiderWebs(6);
	}

	public static void SpectatePlayer(PlayerControllerB player)
	{
		player.Handle().Spectate();
	}

	public static void MiniCam(PlayerControllerB player)
	{
		player.Handle().MiniCam();
	}

	public static void SaveTeleportPosition()
	{
		LethalMenu.localPlayer.Handle().SavePosition();
	}

	public static void TeleportShip()
	{
		LethalMenu.localPlayer.Handle().TeleportShip();
	}

	public static void TeleportSavedPosition()
	{
		LethalMenu.localPlayer.Handle().TeleportSaved();
	}

	public static void Teleport(Vector3 pos, bool elevator = false, bool ship = false, bool factory = false)
	{
		LethalMenu.localPlayer.Handle().Teleport(pos, elevator, ship, factory);
	}

	public static void KillPlayer(PlayerControllerB player)
	{
		player.Handle().Kill();
	}

	public static void HealPlayer(PlayerControllerB player)
	{
		player.Handle().Heal();
	}

	public static void ExplodeJetpack(JetpackItem jetpack)
	{
		RoundHandler.ExplodeJetpack(jetpack);
	}

	public static void DemiGod(PlayerControllerB player)
	{
		DemiGodCheat.ToggleDemiGod(player);
	}

	public static void LightningStrikePlayer(PlayerControllerB player)
	{
		player.Handle().Strike();
	}

	public static void SpawnHoardingBugInfestation()
	{
		RoundHandler.SpawnHoardingBugInfestation();
	}

	public static void ControlEnemy(EnemyAI e)
	{
		e.Handle().Control();
	}

	/// <summary>Makes a monster walk to a player, then lets its normal AI take over (see Cheats.EnemyHunt).</summary>
	public static void HuntPlayer(EnemyAI enemy, PlayerControllerB player)
	{
		string message = Cheats.EnemyHunt.Begin(enemy, player);
		HUDManager.Instance?.DisplayTip("Lethal Menu", message);
	}

	/// <summary>Like Hunt Player, but it never stops by itself: it keeps going at them until you stop it or they die.</summary>
	public static void PersistentHunt(EnemyAI enemy, PlayerControllerB player)
	{
		string message = Cheats.EnemyHunt.Begin(enemy, player, persistent: true);
		HUDManager.Instance?.DisplayTip("Lethal Menu", message);
	}

	/// <summary>Eyeless Dogs only: walk until about 12m from the player, then get enraged, sprint and bite.</summary>
	public static void AggroHunt(EnemyAI enemy, PlayerControllerB player)
	{
		string message = Cheats.EnemyHunt.Begin(enemy, player, persistent: true, aggro: true);
		HUDManager.Instance?.DisplayTip("Lethal Menu", message);
	}

	/// <summary>One-shot hunt on the player with every monster on the map.</summary>
	public static void HuntWithAll(PlayerControllerB player)
	{
		string message = Cheats.EnemyHunt.BeginAll(player);
		HUDManager.Instance?.DisplayTip("Lethal Menu", message);
	}

	public static void StopAllHunts()
	{
		int count = Cheats.EnemyHunt.StopAll();
		HUDManager.Instance?.DisplayTip("Lethal Menu", count == 0 ? "No monsters were hunting" : "Stopped " + count + " hunt" + (count == 1 ? "" : "s"));
	}

	public static void TeleportEnemy(PlayerControllerB player, EnemyAI e)
	{
		e.Handle().Teleport(player);
	}

	public static void TeleportAllEnemies(PlayerControllerB player, EnemyAI[] enemies)
	{
		enemies.ToList().FindAll((EnemyAI e) => !e.isEnemyDead).ForEach((EnemyAI e) =>
		{
			e.Handle().Teleport(player);
		});
	}

	public static void StunAllEnemies()
	{
		RoundHandler.StunAll();
	}

	public static void KillAllEnemies()
	{
		RoundHandler.KillAll();
	}

	public static void KillNearbyEnemies(int distance = -1)
	{
		LethalMenu.enemies.FindAll((EnemyAI e) => GameUtil.GetDistanceToPlayer(e.transform.position) <= (float)distance).ForEach((EnemyAI enemy) =>
		{
			enemy.Handle().Kill();
		});
	}

	public static void ToggleShipLights()
	{
		RoundHandler.ToggleShipLights();
	}

	public static void ToggleCarHorn()
	{
		RoundHandler.ToggleCarHorn();
	}

	public static void ToggleFactoryLights()
	{
		RoundHandler.ToggleFactoryLights();
	}

	public static void TeleportAllItems()
	{
		RoundHandler.TeleportAllItems();
	}

	public static void TeleportOneItem()
	{
		RoundHandler.TeleportOneItem();
	}

	public static void EjectEveryone()
	{
		StartOfRound.Instance.ManuallyEjectPlayersServerRpc();
	}

	public static void JoinLobby(SteamId id)
	{
		RoundHandler.JoinLobby(id);
	}

	public static void Disconnect()
	{
		RoundHandler.Disconnect();
	}

	public static void VoteShipLeaveEarly()
	{
		TimeOfDay.Instance.VoteShipToLeaveEarly();
	}

	public static void BerserkAllTurrets()
	{
		RoundHandler.BerserkAllTurrets();
	}

	public static void DropAllItems()
	{
		RoundHandler.DropAllItems();
	}

	public static void ToggleTerminalSound()
	{
		LethalMenu.Instance.StartCoroutine(RoundHandler.ToggleTerminalSound());
	}

	public static void DeleteHeldItem()
	{
		RoundHandler.DeleteHeldItem();
	}

	public static void UnlockAllDoors()
	{
		RoundHandler.UnlockAllDoors();
	}

	public static void OpenAllBigDoors()
	{
		RoundHandler.OpenAllBigDoors();
	}

	public static void CloseAllBigDoors()
	{
		RoundHandler.CloseAllBigDoors();
	}

	public static void ForceMeteorShower()
	{
		RoundHandler.ForceMeteorShower();
	}

	public static void ClearMeteorShower()
	{
		RoundHandler.ClearMeteorShower();
	}

	public static void ToggleMineshaftElevator()
	{
		RoundHandler.ToggleMineshaftElevator();
	}

	public static void ToggleVehicleMagnet()
	{
		RoundHandler.ToggleVehicleMagnet();
	}

	public static void SpamShootAllShotguns()
	{
		LethalMenu.Instance.StartCoroutine(RoundHandler.SpamShootAllShotguns());
	}

	public static void ShootAllShotguns()
	{
		RoundHandler.ShootAllShotguns();
	}

	public static void ExplodeCruiser()
	{
		RoundHandler.ExplodeCruiser();
	}

	public static void ExplodeJetpacksOnGrab()
	{
		LethalMenu.Instance.StartCoroutine(RoundHandler.ExplodeJetpacksOnGrab());
	}

	public static void ExplodeAllJetpacks()
	{
		RoundHandler.ExplodeAllJetpacks();
	}
}
