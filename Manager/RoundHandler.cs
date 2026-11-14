using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using LethalMenu.Types;
using LethalMenu.Util;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Manager;

public class RoundHandler
{
	public static void ModCredits(int amount, ActionType type)
	{
		if (!(LethalMenu.terminal == null))
		{
			int newGroupCredits = amount;
			if (type == ActionType.Add)
			{
				newGroupCredits = LethalMenu.terminal.groupCredits + amount;
			}
			if (type == ActionType.Remove)
			{
				newGroupCredits = LethalMenu.terminal.groupCredits - amount;
			}
			LethalMenu.terminal.SyncGroupCreditsServerRpc(newGroupCredits, LethalMenu.terminal.numberOfItemsInDropship);
		}
	}

	public static void SetQuota(int amount)
	{
		if (!(TimeOfDay.Instance == null))
		{
			TimeOfDay.Instance.profitQuota = amount;
			TimeOfDay.Instance.SyncNewProfitQuotaClientRpc(TimeOfDay.Instance.profitQuota, 0, TimeOfDay.Instance.timesFulfilledQuota);
		}
	}

	public static void SetDeadline(int amount)
	{
		if (!(TimeOfDay.Instance == null) && !(HUDManager.Instance == null) && PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			if (!StartOfRound.Instance.inShipPhase)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "You must be in orbit!");
				return;
			}
			TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime * (float)amount;
			TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
			TimeOfDay.Instance.SyncTimeClientRpc(TimeOfDay.Instance.globalTime, (int)TimeOfDay.Instance.timeUntilDeadline);
		}
	}

	public static void Message(string msg, int type, int id)
	{
		if (HUDManager.Instance == null || string.IsNullOrEmpty(msg))
		{
			return;
		}
		if (type == 4)
		{
			if (HUDManager.Instance.lastChatMessage == msg)
			{
				msg += "\u200b";
			}
			HUDManager.Instance.AddTextToChatOnServer(msg, id);
		}
		if (type == 3 && !Unlockable.SignalTranslator.GetItem().hasBeenUnlockedByPlayer)
		{
			BuyUnlockable(Unlockable.SignalTranslator, enabled: false);
			HUDManager.Instance.UseSignalTranslatorServerRpc(msg);
		}
		else if (type == 3 && Unlockable.SignalTranslator.GetItem().hasBeenUnlockedByPlayer)
		{
			HUDManager.Instance.UseSignalTranslatorServerRpc(msg);
		}
		if (type <= 2)
		{
			string[] array = new string[3] { "[System]", "[Server]", "[Broadcast]" };
			string text = array[type] + " " + msg;
			if (HUDManager.Instance.lastChatMessage == text)
			{
				text += "\u200b";
			}
			HUDManager.Instance.AddTextToChatOnServer(text);
		}
	}

	public static void BuyUnlockable(Unlockable unlockable, bool enabled = true)
	{
		if (!(StartOfRound.Instance == null) && !(HUDManager.Instance == null) && !(LethalMenu.terminal == null))
		{
			unlockable.Buy(LethalMenu.terminal.groupCredits);
			if (enabled)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "Unlocked " + unlockable.GetItem().unlockableName + "!");
			}
		}
	}

	public static bool ToggleShipLights()
	{
		if (!StartOfRound.Instance)
		{
			return false;
		}
		ShipLights shipLights = UnityEngine.Object.FindObjectOfType<ShipLights>();
		shipLights.ToggleShipLights();
		return shipLights.areLightsOn;
	}

	public static bool ToggleFactoryLights()
	{
		if (!StartOfRound.Instance || !RoundManager.Instance || LethalMenu.breaker == null)
		{
			return false;
		}
		if (RoundManager.Instance.powerOffPermanently)
		{
			RoundManager.Instance.powerOffPermanently = false;
		}
		RoundManager.Instance.SwitchPower(!LethalMenu.breaker.isPowerOn);
		return LethalMenu.breaker.isPowerOn;
	}

	public static bool AreFactoryLightsOn()
	{
		if (!StartOfRound.Instance || LethalMenu.breaker == null)
		{
			return false;
		}
		return LethalMenu.breaker.isPowerOn;
	}

	public static bool AreShipLightsOn()
	{
		if (!StartOfRound.Instance)
		{
			return false;
		}
		ShipLights shipLights = UnityEngine.Object.FindObjectOfType<ShipLights>();
		return shipLights.areLightsOn;
	}

	public static void StartGame()
	{
		StartOfRound.Instance?.StartGameServerRpc();
	}

	public static void EndGame()
	{
		StartOfRound.Instance?.EndGameServerRpc(-1);
	}

	public static void ResetShip()
	{
		if ((bool)StartOfRound.Instance)
		{
			if (!StartOfRound.Instance.inShipPhase)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "You must be in orbit!");
			}
			else
			{
				StartOfRound.Instance.ResetShip();
			}
		}
	}

	public static void SpawnScrap()
	{
		if (!(RoundManager.Instance == null) && !(RoundManager.Instance.currentLevel == null) && RoundManager.Instance.currentLevel.spawnEnemiesAndScrap)
		{
			RoundManager.Instance.SpawnScrapInLevel();
		}
	}

	public static void ModScrap(int value, int type)
	{
		if ((bool)StartOfRound.Instance && (bool)RoundManager.Instance)
		{
			if (type == 0)
			{
				RoundManager.Instance.scrapAmountMultiplier = value;
			}
			if (type == 1)
			{
				RoundManager.Instance.scrapValueMultiplier = value;
			}
		}
	}

	public static void ForceTentacleAttack()
	{
		if (!(LethalMenu.depositItemsDesk == null))
		{
			LethalMenu.depositItemsDesk.AttackPlayersServerRpc();
		}
	}

	public static void ForceBridgeFall()
	{
		BridgeTrigger bridgeTrigger = UnityEngine.Object.FindObjectOfType(typeof(BridgeTrigger)) as BridgeTrigger;
		if (!(bridgeTrigger == null))
		{
			bridgeTrigger.BridgeFallServerRpc();
		}
	}

	public static void ForceSmallBridgeFall()
	{
		BridgeTriggerType2 bridgeTriggerType = UnityEngine.Object.FindObjectOfType(typeof(BridgeTriggerType2)) as BridgeTriggerType2;
		if (!(bridgeTriggerType == null))
		{
			for (int i = 0; i < 4; i++)
			{
				bridgeTriggerType.AddToBridgeInstabilityServerRpc();
			}
		}
	}

	public static void SpawnHoardingBugInfestation()
	{
		if (RoundManager.Instance == null || !PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			return;
		}
		for (int i = 0; i < RoundManager.Instance.currentLevel.Enemies.Count; i++)
		{
			if (RoundManager.Instance.currentLevel.Enemies.Any((SpawnableEnemyWithRarity e) => e.enemyType.enemyName == "Hoarding bug"))
			{
				SpawnEnemy(GameUtil.GetEnemyTypes().ToList().FirstOrDefault((EnemyType e) => e.enemyName == "Hoarding bug"), 1, outside: false);
			}
			else
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "There must be one Hoarding bug spawned");
			}
			if (RoundManager.Instance.currentLevel.Enemies[i].enemyType.enemyName == "Hoarding bug")
			{
				RoundManager.Instance.Reflect().SetValue("enemyRushIndex", i);
				RoundManager.Instance.currentMaxInsidePower = 30f;
			}
		}
	}

	public static void KillAll()
	{
		if (HUDManager.Instance == null)
		{
			return;
		}
		if (LethalMenu.enemies.Count == 0)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "No enemies to kill!");
			return;
		}
		HUDManager.Instance.DisplayTip("Lethal Menu", $"Killed {LethalMenu.enemies.Count} enemies!");
		LethalMenu.enemies.Where((EnemyAI e) => e != null && !e.isEnemyDead).ToList().ForEach((EnemyAI e) =>
		{
			e.Handle().Kill(despawn: false, NoAlert: true);
		});
	}

	public static void StunAll()
	{
		if (HUDManager.Instance == null)
		{
			return;
		}
		if (LethalMenu.enemies.Count == 0)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "No enemies to stun!");
			return;
		}
		HUDManager.Instance.DisplayTip("Lethal Menu", $"Stunned {LethalMenu.enemies.Count} enemies!");
		LethalMenu.enemies.Where((EnemyAI e) => e != null && !e.isEnemyDead).ToList().ForEach((EnemyAI e) =>
		{
			e.Handle().Stun(NoAlert: true);
		});
	}

	public static void TeleportAllItems()
	{
		LethalMenu.items.FindAll((GrabbableObject i) => i != null && !i.isHeld && !i.isPocketed && !i.isInShipRoom && !i.heldByPlayerOnServer).ForEach((GrabbableObject i) =>
		{
			Vector3 point = new Ray(LethalMenu.localPlayer.gameplayCamera.transform.position, LethalMenu.localPlayer.gameplayCamera.transform.forward).GetPoint(1f);
			i.gameObject.transform.position = point;
			i.startFallingPosition = point;
			i.targetFloorPosition = point;
		});
	}

	public static void TeleportOneItem()
	{
		GrabbableObject grabbableObject = (from i in LethalMenu.items
			where i != null && !i.isHeld && !i.isPocketed && !i.isInShipRoom && !i.heldByPlayerOnServer
			orderby UnityEngine.Random.value
			select i).FirstOrDefault();
		Vector3 point = new Ray(LethalMenu.localPlayer.gameplayCamera.transform.position, LethalMenu.localPlayer.gameplayCamera.transform.forward).GetPoint(1f);
		grabbableObject.gameObject.transform.position = point;
		grabbableObject.startFallingPosition = point;
		grabbableObject.targetFloorPosition = point;
	}

	public static void JoinLobby(SteamId id)
	{
		GameNetworkManager.Instance.StartClient(id);
	}

	public static void Disconnect()
	{
		GameNetworkManager.Instance.Disconnect();
	}

	public static void DropAllItems()
	{
		Settings.b_DropItems = true;
		if (Settings.b_DropItems)
		{
			LethalMenu.localPlayer.DropAllHeldItemsAndSyncNonexact();
		}
		Settings.b_DropItems = false;
	}

	public static void DeleteHeldItem()
	{
		LethalMenu.localPlayer.DespawnHeldObject();
	}

	public static void ToggleShipHorn()
	{
		ShipAlarmCord shipAlarmCord = UnityEngine.Object.FindObjectOfType<ShipAlarmCord>();
		if (!(shipAlarmCord == null))
		{
			if (Hack.ToggleShipHorn.IsEnabled())
			{
				shipAlarmCord.PullCordServerRpc(-1);
			}
			else
			{
				shipAlarmCord.StopPullingCordServerRpc(-1);
			}
		}
	}

	public static void ToggleCarHorn()
	{
		if (LethalMenu.vehicles == null || LethalMenu.vehicles.Count == 0)
		{
			return;
		}
		LethalMenu.vehicles.Where((VehicleController v) => v != null).ToList().ForEach((VehicleController v) =>
		{
			if (Hack.ToggleCarHorn.IsEnabled())
			{
				v.SetHonkServerRpc(honk: true, -1);
			}
			else
			{
				v.SetHonkServerRpc(honk: false, -1);
			}
		});
	}

	/// <summary>
	/// Spawn Masked Enemy. Every Comedy/Tragedy mask on the moon is turned into a Masked (the game's own
	/// "mask takes over" spawn). If there are no masks, the host spawns one Masked next to themselves instead;
	/// before, that case silently did nothing.
	/// </summary>
	public static void SpawnMimicFromMasks()
	{
		void Tip(string text)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", text);
		}
		if (StartOfRound.Instance == null || RoundManager.Instance == null || LethalMenu.localPlayer == null)
		{
			return;
		}
		if (StartOfRound.Instance.inShipPhase)
		{
			Tip("Land on a moon first: a Masked can't be spawned in orbit");
			return;
		}
		PlayerControllerB[] alive = StartOfRound.Instance.allPlayerScripts.Where((PlayerControllerB p) => p != null && p.isPlayerControlled && !p.isPlayerDead).ToArray();
		if (alive.Length == 0)
		{
			Tip("No living player for the Masked to copy");
			return;
		}
		PlayerControllerB copied = alive[UnityEngine.Random.Range(0, alive.Length)];
		bool host = PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer);

		List<HauntedMaskItem> masks = LethalMenu.items.Where((GrabbableObject i) => i != null && i is HauntedMaskItem && i.NetworkObject != null && i.NetworkObject.IsSpawned).Cast<HauntedMaskItem>().ToList();
		int converted = 0;
		foreach (HauntedMaskItem mask in masks)
		{
			try
			{
				mask.ChangeOwnershipOfProp(LethalMenu.localPlayer.actualClientId);
				mask.Reflect().SetValue("previousPlayerHeldBy", copied);
				mask.CreateMimicServerRpc(mask.isInFactory, mask.transform.position);
				converted++;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Mask to Masked failed: " + ex.Message);
			}
		}
		if (converted > 0)
		{
			Tip("Turned " + converted + " mask" + (converted == 1 ? "" : "s") + " into Masked" + (host ? "" : " (as a client this only works for masks someone has held)"));
			return;
		}
		if (!host)
		{
			Tip("There are no masks on this moon. Without one, only the host can spawn a Masked");
			return;
		}

		// No masks: spawn a Masked directly, a few steps in front of you (or of whoever is alive if you are dead).
		EnemyType type = GameUtil.GetEnemyTypes().FirstOrDefault((EnemyType e) => e != null && e.enemyPrefab != null && e.enemyPrefab.GetComponent<MaskedPlayerEnemy>() != null);
		if (type == null)
		{
			Tip("Couldn't find the Masked in the game's monster list");
			return;
		}
		PlayerControllerB anchor = LethalMenu.localPlayer.isPlayerDead ? copied : LethalMenu.localPlayer;
		Vector3 wanted = anchor.transform.position + anchor.transform.forward * 4f;
		Vector3 spot = RoundManager.Instance.GetNavMeshPosition(wanted, default(UnityEngine.AI.NavMeshHit), 10f);
		if (!RoundManager.Instance.GotNavMeshPositionResult)
		{
			// Standing somewhere monsters can't walk (inside the ship): use one of the map's monster waypoints.
			GameObject[] nodes = anchor.isInsideFactory ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes;
			if (nodes == null || nodes.Length == 0)
			{
				Tip("No walkable ground found to spawn a Masked on");
				return;
			}
			spot = nodes.OrderBy((GameObject n) => Vector3.Distance(n.transform.position, anchor.transform.position)).First().transform.position;
		}
		NetworkObjectReference reference = RoundManager.Instance.SpawnEnemyGameObject(spot, anchor.transform.eulerAngles.y + 180f, -1, type);
		if (reference.TryGet(out NetworkObject spawned) && spawned.TryGetComponent<MaskedPlayerEnemy>(out MaskedPlayerEnemy masked))
		{
			// Same setup the game does when a mask takes someone over.
			masked.SetSuit(copied.currentSuitID);
			masked.mimickingPlayer = copied;
			masked.SetEnemyOutside(!anchor.isInsideFactory);
			masked.SetVisibilityOfMaskedEnemy();
			Tip("Spawned a Masked near " + (anchor == LethalMenu.localPlayer ? "you" : anchor.playerUsername) + ", copying " + copied.playerUsername);
		}
		else
		{
			Tip("The game refused to spawn a Masked here");
		}
	}

	public static IEnumerator SpamShootAllShotguns()
	{
		while (Hack.SpamShootAllShotguns.IsEnabled())
		{
			ShootAllShotguns();
			yield return new WaitForSeconds(0.1f);
		}
	}

	public static void ShootAllShotguns()
	{
		LethalMenu.items.FindAll((GrabbableObject i) => i != null && i.NetworkObject != null && i.NetworkObject.IsSpawned && i.GetType() == typeof(ShotgunItem)).Cast<ShotgunItem>().ToList()
			.ForEach((ShotgunItem m) =>
			{
				PlayerControllerB value = m.Reflect().GetValue<PlayerControllerB>("previousPlayerHeldBy");
				Vector3 shotgunPosition = ((value != null) ? (value.gameplayCamera.transform.position - value.gameplayCamera.transform.up * 0.45f) : (m.transform.up * 0.45f));
				Vector3 shotgunForward = ((value != null) ? value.gameplayCamera.transform.forward : m.transform.forward);
				m.ShootGunServerRpc(shotgunPosition, shotgunForward);
			});
	}

	public static void ExplodeCruiser()
	{
		if (LethalMenu.vehicles != null && LethalMenu.vehicles.Count != 0)
		{
			LethalMenu.vehicles.Where((VehicleController v) => v != null).ToList().ForEach((VehicleController v) =>
			{
				v.DestroyCarServerRpc(-1);
			});
		}
	}

	public static void ExplodeJetpack(JetpackItem jetpack)
	{
		if (!(jetpack == null))
		{
			PlayerControllerB playerHeldBy = jetpack.playerHeldBy;
			if (playerHeldBy != null)
			{
				playerHeldBy.DropAllHeldItems(true, false);
			}
			jetpack.ExplodeJetpackServerRpc();
			if (playerHeldBy != null)
			{
				jetpack.DestroyObjectInHand(playerHeldBy);
			}
		}
	}

	public static void SpawnEnemy(EnemyType type, int num, bool outside)
	{
		if (StartOfRound.Instance.inShipPhase)
		{
			return;
		}
		SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
		currentLevel.maxEnemyPowerCount = int.MaxValue;
		GameObject[] array = (outside ? RoundManager.Instance.outsideAINodes : RoundManager.Instance.insideAINodes);
		List<Transform> nodes = new List<Transform>();
		Array.ForEach(array, (GameObject obj) =>
		{
			nodes.Add(obj.transform);
		});
		for (int num2 = 0; num2 < num; num2++)
		{
			Transform transform = nodes[UnityEngine.Random.Range(0, nodes.Count)];
			if (type.enemyName == "Bush Wolf")
			{
				LethalMenu.Instance.StartCoroutine(SpawnBushWolf(type));
			}
			else
			{
				RoundManager.Instance.SpawnEnemyGameObject(transform.position, 0f, -1, type);
			}
		}
	}

	private static SpawnableMapObject FindMapObject(MapObject type)
	{
		SpawnableMapObject found = GameUtil.GetSpawnableMapObjects().FirstOrDefault((SpawnableMapObject o) => o != null && o.prefabToSpawn != null && o.prefabToSpawn.name == type.ToString());
		if (found == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "The game has no " + type + " to spawn");
		}
		return found;
	}

	/// <summary>
	/// Where the game keeps spawned traps. The game only looks this up when it spawns traps itself, and the old
	/// one is destroyed with the previous moon, so find it again when it is missing (no parent if there is none).
	/// </summary>
	private static Transform MapPropsParent()
	{
		if (RoundManager.Instance.mapPropsContainer == null)
		{
			RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
		}
		return RoundManager.Instance.mapPropsContainer != null ? RoundManager.Instance.mapPropsContainer.transform : null;
	}

	public static void SpawnMapObject(MapObject type)
	{
		if (!(LethalMenu.localPlayer == null) && RoundManager.Instance.AnomalyRandom != null && !StartOfRound.Instance.inShipPhase)
		{
			Vector3 vector = LethalMenu.localPlayer.transform.position + LethalMenu.localPlayer.transform.forward * 2f;
			SpawnableMapObject spawnableMapObject = FindMapObject(type);
			if (spawnableMapObject == null)
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(spawnableMapObject.prefabToSpawn, vector, Quaternion.identity, MapPropsParent());
			gameObject.transform.eulerAngles = ((!spawnableMapObject.spawnFacingAwayFromWall) ? new Vector3(gameObject.transform.eulerAngles.x, RoundManager.Instance.AnomalyRandom.Next(0, 360), gameObject.transform.eulerAngles.z) : new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(vector + Vector3.up * 0.2f), 0f));
			gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
		}
	}

	public static void SpawnMapObjects(MapObject type)
	{
		if (!(LethalMenu.localPlayer == null) && RoundManager.Instance.AnomalyRandom != null && !StartOfRound.Instance.inShipPhase)
		{
			SpawnableMapObject spawnableMapObject = FindMapObject(type);
			if (spawnableMapObject == null || RoundManager.Instance.insideAINodes == null || RoundManager.Instance.insideAINodes.Length == 0)
			{
				return;
			}
			int num = UnityEngine.Random.Range(5, 15);
			for (int num2 = 0; num2 < num; num2++)
			{
				GameObject gameObject = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)];
				Vector3 randomNavMeshPositionInRadius = RoundManager.Instance.GetRandomNavMeshPositionInRadius(gameObject.transform.position, 30f);
				GameObject gameObject2 = UnityEngine.Object.Instantiate(spawnableMapObject.prefabToSpawn, randomNavMeshPositionInRadius, Quaternion.identity, MapPropsParent());
				gameObject2.transform.eulerAngles = ((!spawnableMapObject.spawnFacingAwayFromWall) ? new Vector3(gameObject2.transform.eulerAngles.x, RoundManager.Instance.AnomalyRandom.Next(0, 360), gameObject2.transform.eulerAngles.z) : new Vector3(0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(randomNavMeshPositionInRadius + Vector3.up * 0.2f), 0f));
				gameObject2.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
			}
		}
	}

	public static IEnumerator SpawnBushWolf(EnemyType type)
	{
		MoldSpreadManager moldSpreadManager = UnityEngine.Object.FindObjectOfType<MoldSpreadManager>();
		if (!(moldSpreadManager == null))
		{
			moldSpreadManager.GenerateMold(RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position, 1);
			yield return new WaitForSeconds(0.5f);
			moldSpreadManager.RemoveAllMold();
		}
	}

	public static IEnumerator ToggleTerminalSound()
	{
		while (Hack.ToggleTerminalSound.IsEnabled())
		{
			if (LethalMenu.terminal != null)
			{
				LethalMenu.terminal.PlayTerminalAudioServerRpc(1);
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	public static IEnumerator ExplodeJetpacksOnGrab()
	{
		while (Hack.ExplodeJetpacksOnGrab.IsEnabled())
		{
			(from JetpackItem j in LethalMenu.items.FindAll((GrabbableObject i) => i != null && i.GetType() == typeof(JetpackItem))
				where j != null && j.isHeld
				select j).ToList().ForEach((JetpackItem j) =>
			{
				ExplodeJetpack(j);
			});
			yield return new WaitForSeconds(0.1f);
		}
	}

	public static void ExplodeAllJetpacks()
	{
		(from JetpackItem j in LethalMenu.items.FindAll((GrabbableObject i) => i != null && i.GetType() == typeof(JetpackItem))
			where j != null && !j.isHeld
			select j).ToList().ForEach((JetpackItem j) =>
		{
			ExplodeJetpack(j);
		});
	}

	public static void BreakAllWebs()
	{
		LethalMenu.enemies.FindAll((EnemyAI e) => e.GetType() == typeof(SandSpiderAI)).Cast<SandSpiderAI>().ToList()
			.ForEach((SandSpiderAI s) =>
			{
				s.BreakAllWebs();
			});
	}

	public static void UnlockAllDoors()
	{
		if (LethalMenu.doorLocks == null || HUDManager.Instance == null)
		{
			return;
		}
		List<DoorLock> list = LethalMenu.doorLocks.Where((DoorLock d) => d != null && d.isLocked).ToList();
		if (list.Count == 0)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "No doors to unlock!");
			return;
		}
		list.ToList().ForEach((DoorLock d) =>
		{
			d.UnlockDoorSyncWithServer();
		});
		string arg = ((list.Count == 1) ? "door" : "doors");
		HUDManager.Instance.DisplayTip("Lethal Menu", $"Unlocked {list.Count} {arg}!");
	}

	public static void OpenAllBigDoors()
	{
		if (LethalMenu.bigDoors == null || HUDManager.Instance == null || RoundManager.Instance == null)
		{
			return;
		}
		LethalMenu.bigDoors.Where((TerminalAccessibleObject d) => d != null).ToList().ForEach((TerminalAccessibleObject d) =>
		{
			if (!d.Reflect().GetValue<bool>("isPoweredOn"))
			{
				d.OnPowerSwitch(switchedOn: true);
			}
			d.SetDoorOpenServerRpc(open: true);
		});
		HUDManager.Instance.DisplayTip("Lethal Menu", "All Big Doors Opened");
	}

	public static void CloseAllBigDoors()
	{
		if (LethalMenu.bigDoors == null || HUDManager.Instance == null)
		{
			return;
		}
		LethalMenu.bigDoors.Where((TerminalAccessibleObject d) => d != null).ToList().ForEach((TerminalAccessibleObject d) =>
		{
			if (!d.Reflect().GetValue<bool>("isPoweredOn"))
			{
				d.OnPowerSwitch(switchedOn: true);
			}
			d.SetDoorOpenServerRpc(open: false);
		});
		HUDManager.Instance.DisplayTip("Lethal Menu", "All Big Doors Closed");
	}

	public static void ToggleMineshaftElevator()
	{
		if (!(LethalMenu.mineshaftElevator == null))
		{
			LethalMenu.mineshaftElevator.ElevatorFinishClientRpc(atTop: true);
			LethalMenu.mineshaftElevator.PressElevatorButtonServerRpc();
		}
	}

	public static void ToggleVehicleMagnet()
	{
		if (!(StartOfRound.Instance == null))
		{
			StartOfRound.Instance.SetMagnetOnServerRpc(!StartOfRound.Instance.magnetOn);
		}
	}

	public static void ForceMeteorShower()
	{
		if (PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer) && !(TimeOfDay.Instance == null))
		{
			TimeOfDay.Instance.MeteorWeather.BeginDay(TimeOfDay.Instance.normalizedTimeOfDay);
		}
	}

	public static void ClearMeteorShower()
	{
		if (TimeOfDay.Instance.MeteorWeather.meteors.Count != 0 && PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer) && !(TimeOfDay.Instance == null))
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", $"Cleared {TimeOfDay.Instance.MeteorWeather.meteors.Count} Meteors");
			TimeOfDay.Instance.MeteorWeather.ResetMeteorWeather();
		}
	}

	public static void ChangeMoon(int levelID)
	{
		if ((bool)StartOfRound.Instance)
		{
			StartOfRound.Instance.ChangeLevelServerRpc(levelID, LethalMenu.terminal.groupCredits);
		}
	}

	public static void UnlockObjects()
	{
		if (!Hack.UnlockObjects.IsEnabled() || CameraManager.ActiveCamera == null || !Physics.Raycast(CameraManager.ActiveCamera.transform.position, CameraManager.ActiveCamera.transform.forward, out var hitInfo, 5f, LayerMask.GetMask("InteractableObject") | LayerMask.GetMask("Default") | LayerMask.GetMask("MapHazards")))
		{
			return;
		}
		DoorLock component = hitInfo.transform.GetComponent<DoorLock>();
		TerminalAccessibleObject component2 = hitInfo.transform.GetComponent<TerminalAccessibleObject>();
		if (component != null && component.isLocked && !component.isPickingLock)
		{
			component.UnlockDoorSyncWithServer();
			HUDManager.Instance.DisplayTip("Lethal Menu", "Door Unlocked");
		}
		else if (component2 != null)
		{
			if (!component2.Reflect().GetValue<bool>("isPoweredOn"))
			{
				component2.OnPowerSwitch(switchedOn: true);
			}
			if (component2.Reflect().GetValue<bool>("inCooldown"))
			{
				component2.Reflect().SetValue("inCooldown", false);
			}
			string text;
			if (component2.isBigDoor)
			{
				text = "Big Door";
			}
			else if (component2.name == "TurretScript")
			{
				text = "Turret";
			}
			else
			{
				text = ((component2.name == "Landmine") ? "Landmine" : "Terminal Object");
			}
			component2.CallFunctionFromTerminal();
			HUDManager.Instance.DisplayTip("Lethal Menu", text + " ( " + component2.objectCode + " ) has been called from the terminal");
		}
	}

	public static void ClickKill()
	{
		if (Hack.ClickKill.IsEnabled() && !Hack.ClickTeleport.IsEnabled() && !(CameraManager.ActiveCamera == null) && Physics.Raycast(CameraManager.ActiveCamera.transform.position, CameraManager.ActiveCamera.transform.forward, out var hitInfo, 1000f, LayerMask.GetMask("Enemies")) && !(hitInfo.transform == null))
		{
			EnemyAI enemyAI = hitInfo.transform.GetComponentInParent<EnemyAI>() ?? hitInfo.transform.GetComponentInChildren<EnemyAI>();
			if (!(enemyAI == null) && !enemyAI.isEnemyDead)
			{
				enemyAI?.Handle()?.Kill();
			}
		}
	}

	public static void ClickTeleport()
	{
		if (Hack.ClickTeleport.IsEnabled() && !Hack.ClickKill.IsEnabled() && !(CameraManager.ActiveCamera == null) && Physics.Raycast(CameraManager.ActiveCamera.transform.position, CameraManager.ActiveCamera.transform.forward, out var hitInfo, 1000f, LayerMask.GetMask("Room")))
		{
			_ = hitInfo.point;
			if (LethalMenu.localPlayer != null)
			{
				LethalMenu.localPlayer?.Handle()?.Teleport(hitInfo.point);
			}
		}
	}

	public static void FixAllValves()
	{
		LethalMenu.steamValves.Where((SteamValveHazard s) => s != null && s.fixInteract != null && !s.Reflect().GetValue<bool>("valveHasBeenRepaired")).ToList().ForEach((SteamValveHazard v) =>
		{
			v.fixInteract.FixValveServerRpc();
		});
	}

	public static void ToggleAllLandmines()
	{
		LethalMenu.landmines.Where((Landmine l) => l != null).ToList().ForEach((Landmine m) =>
		{
			m.ToggleMine(!Hack.ToggleAllLandmines.IsEnabled());
		});
	}

	public static void ToggleAllTurrets()
	{
		LethalMenu.turrets.Where((Turret t) => t != null).ToList().ForEach((Turret t) =>
		{
			t.turretActive = !Hack.ToggleAllTurrets.IsEnabled();
		});
	}

	public static void BerserkAllTurrets()
	{
		LethalMenu.turrets.Where((Turret t) => t != null).ToList().ForEach((Turret t) =>
		{
			t.turretMode = (Hack.BerserkAllTurrets.IsEnabled() ? TurretMode.Berserk : TurretMode.Detection);
		});
	}

	public static void BlowUpAllLandmines()
	{
		LethalMenu.landmines.Where((Landmine m) => m != null).ToList().ForEach((Landmine m) =>
		{
			m.ExplodeMineServerRpc();
		});
	}
}
