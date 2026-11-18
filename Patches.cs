using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Cheats;
using LethalMenu.Manager;
using LethalMenu.Menu.Tab;
using LethalMenu.Util;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu;

[HarmonyPatch]
internal class Patches
{
	public static bool SellQuota;

	[HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesClientRpc")]
	[HarmonyPostfix]
	public static void SendNewPlayerValuesClientRpc(PlayerControllerB __instance)
	{
		if (__instance.IsLocalPlayer)
		{
			LethalMenu.Instance.StartCoroutine(LocalPlayerJoin());
		}
	}

	private static IEnumerator LocalPlayerJoin()
	{
		yield return new WaitForSeconds(2f);
		LethalMenu.items.Where((GrabbableObject i) => i != null && !i.isInShipRoom).ToList().ForEach((GrabbableObject i) =>
		{
			i.isInShipRoom = true;
		});
	}

	[HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
	[HarmonyPostfix]
	public static void Disconnect(GameNetworkManager __instance)
	{
		// The monster may already be gone when you leave; a failure here must not skip the rest of the cleanup.
		try
		{
			EnemyControl.StopControl();
			EnemyHunt.StopAll();
		}
		catch (System.Exception ex)
		{
			UnityEngine.Debug.LogWarning("[LethalMenu] Stop control on disconnect: " + ex.Message);
		}
		ObjectManager.ClearObjects();
		SpectatePlayer.Reset();
		Freecam.Reset();
		Shoplifter.Clear();
		ServerTab.UpdatePlayerOptions(clear: true);
		UnlimitedPresents.stuckpresent = false;
	}

	[HarmonyPatch(typeof(StartOfRound), "OnPlayerDC")]
	[HarmonyPostfix]
	public static void OnPlayerDC(StartOfRound __instance, int playerObjectNumber, ulong clientId)
	{
		__instance.allPlayerScripts[clientId].playerSteamId = 0uL;
	}

	[HarmonyPatch(typeof(GameNetworkManager), "StartClient")]
	[HarmonyPostfix]
	public static void StartClient(SteamId id)
	{
		Settings.s_lobbyid = id;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "CloseQuickMenu")]
	[HarmonyPostfix]
	public static void CloseQuickMenu(QuickMenuManager __instance)
	{
		if (Hack.OpenMenu.IsEnabled() && !Cursor.visible)
		{
			MenuUtil.ShowCursor();
		}
		if (LethalMenu.quickMenuManager == null)
		{
			ObjectManager.AddToObjectQueue(() =>
			{
				LethalMenu.quickMenuManager = __instance;
			});
		}
	}

	[HarmonyPatch(typeof(DepositItemsDesk), "AttackPlayersServerRpc")]
	[HarmonyPrefix]
	public static void CompanyAttackPrefix(ref bool ___attacking, ref bool ___inGrabbingObjectsAnimation, ref bool __state)
	{
		__state = ___inGrabbingObjectsAnimation;
		___attacking = false;
		___inGrabbingObjectsAnimation = false;
	}

	[HarmonyPatch(typeof(DepositItemsDesk), "AttackPlayersServerRpc")]
	[HarmonyPostfix]
	public static void CompanyAttackPostfix(ref bool ___inGrabbingObjectsAnimation, ref bool __state)
	{
		___inGrabbingObjectsAnimation = __state;
	}

	[HarmonyPatch(typeof(HUDManager), "DisplayCreditsEarning")]
	[HarmonyPostfix]
	public static void DisplayCreditsEarning(HUDManager __instance, int creditsEarned, GrabbableObject[] objectsSold, int newGroupCredits)
	{
		if (SellQuota)
		{
			int profitQuota = TimeOfDay.Instance.profitQuota;
			int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
			int num = profitQuota - quotaFulfilled;
			if (num > 0)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", $"Not enough items to meet quota! {quotaFulfilled}/{profitQuota}");
			}
			if (num < 0)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", $"Quota met! {quotaFulfilled}/{profitQuota}");
			}
			SellQuota = false;
		}
	}

	[HarmonyPatch(typeof(DepositItemsDesk), "PlaceItemOnCounter")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> PlaceItemOnCounter(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			yield return (instruction.opcode == OpCodes.Ldc_I4_S && instruction.operand.ToString().Equals("12")) ? new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue) : instruction;
		}
	}

	[HarmonyPatch(typeof(QuickMenuManager), "CanEnableDebugMenu")]
	[HarmonyPrefix]
	public static bool CanEnableDebugMenu(ref bool __result)
	{
		if (Settings.DebugMode)
		{
			__result = true;
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
	[HarmonyPrefix]
	public static bool AllowPlayerDeath(PlayerControllerB __instance, ref bool __result)
	{
		if (Settings.DebugMode)
		{
			__result = true;
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_SpawnEnemy")]
	[HarmonyPrefix]
	public static bool Debug_SpawnEnemy(QuickMenuManager __instance)
	{
		if (Settings.DebugMode)
		{
			EnemyType enemyType = null;
			Vector3 spawnPosition = Vector3.zero;
			int value = __instance.Reflect().GetValue<int>("enemyTypeId");
			int value2 = __instance.Reflect().GetValue<int>("enemyToSpawnId");
			int value3 = __instance.Reflect().GetValue<int>("numberEnemyToSpawn");
			switch (value)
			{
			case 0:
				enemyType = __instance.testAllEnemiesLevel.Enemies[value2].enemyType;
				spawnPosition = ((!(StartOfRound.Instance.testRoom != null)) ? RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position : __instance.debugEnemySpawnPositions[value].position);
				break;
			case 1:
				enemyType = __instance.testAllEnemiesLevel.OutsideEnemies[value2].enemyType;
				spawnPosition = ((!(StartOfRound.Instance.testRoom != null)) ? RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position : __instance.debugEnemySpawnPositions[value].position);
				break;
			case 2:
				enemyType = __instance.testAllEnemiesLevel.DaytimeEnemies[value2].enemyType;
				spawnPosition = ((!(StartOfRound.Instance.testRoom != null)) ? RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position : __instance.debugEnemySpawnPositions[value].position);
				break;
			}
			if (!(enemyType == null))
			{
				for (int i = 0; i < value3 && i <= 50; i++)
				{
					if (enemyType.enemyName == "Bush Wolf")
					{
						RoundHandler.SpawnBushWolf(enemyType);
						continue;
					}
					RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0f, -1, enemyType);
				}
			}
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Start")]
	[HarmonyPrefix]
	public static bool Start(QuickMenuManager __instance)
	{
		if (Settings.DebugMode)
		{
			__instance.Reflect().SetValue("currentMicrophoneDevice", PlayerPrefs.GetInt("LethalCompany_currentMic", 0));
			__instance.Reflect().Invoke("Debug_SetEnemyDropdownOptions");
			Debug_SetAllItemsDropdownOptions();
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_SpawnItem")]
	[HarmonyPrefix]
	public static bool Debug_SpawnItem(QuickMenuManager __instance)
	{
		if (Settings.DebugMode)
		{
			int value = __instance.Reflect().GetValue<int>("itemToSpawnId");
			Vector3 position = LethalMenu.localPlayer.playerEye.transform.position;
			if (StartOfRound.Instance.allItemsList.itemsList[value].spawnPrefab == null || StartOfRound.Instance.propsContainer == null)
			{
				return true;
			}
			GameObject gameObject = Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[value].spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
			gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
			gameObject.GetComponent<NetworkObject>().Spawn();
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_KillLocalPlayer")]
	[HarmonyPrefix]
	public static bool Debug_KillLocalPlayer()
	{
		if (Settings.DebugMode)
		{
			GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 0, default(Vector3));
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_SpawnTruck")]
	[HarmonyPrefix]
	public static bool Debug_SpawnTruck(QuickMenuManager __instance)
	{
		if (Settings.DebugMode)
		{
			Object.Instantiate(__instance.truckPrefab, StartOfRound.Instance.groundOutsideShipSpawnPosition.position, Quaternion.identity, RoundManager.Instance.VehiclesContainer).gameObject.GetComponent<NetworkObject>().Spawn();
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_ToggleTestRoom")]
	[HarmonyPrefix]
	public static bool Debug_ToggleTestRoom()
	{
		if (Settings.DebugMode)
		{
			StartOfRound.Instance.Debug_EnableTestRoomServerRpc(StartOfRound.Instance.testRoom == null);
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(StartOfRound), "IsClientFriendsWithHost")]
	[HarmonyPrefix]
	public static bool IsClientFriendsWithHost(ref bool __result)
	{
		if (Settings.DebugMode)
		{
			__result = true;
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(QuickMenuManager), "Debug_ToggleAllowDeath")]
	[HarmonyPrefix]
	public static bool Debug_ToggleAllowDeath()
	{
		if (Settings.DebugMode)
		{
			StartOfRound.Instance.Debug_ToggleAllowDeathServerRpc();
			return false;
		}
		return true;
	}

	public static void Debug_SetAllItemsDropdownOptions()
	{
		if (!(LethalMenu.quickMenuManager == null))
		{
			LethalMenu.quickMenuManager.allItemsDropdown.ClearOptions();
			List<string> list = new List<string>();
			for (int i = 0; i < StartOfRound.Instance.allItemsList.itemsList.Count; i++)
			{
				list.Add(StartOfRound.Instance.allItemsList.itemsList[i].itemName);
			}
			LethalMenu.quickMenuManager.allItemsDropdown.AddOptions(list);
		}
	}
}
