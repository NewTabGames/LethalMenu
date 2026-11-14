using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LethalMenu.Manager;

[HarmonyPatch]
public class ObjectManager
{
	private static Queue<Action> ImportantObjectQueue = new Queue<Action>();

	private static Queue<Action> ObjectQueue = new Queue<Action>();

	private static bool CoroutineStarted = false;

	private static bool ImportantCoroutineStarted = false;

	[HarmonyPatch(typeof(QuickMenuManager), "AddUserToPlayerList")]
	[HarmonyPostfix]
	private static void AddUserToPlayerList(QuickMenuManager __instance)
	{
		if (LethalMenu.quickMenuManager == null)
		{
			AddToImportantObjectQueue(() =>
			{
				LethalMenu.quickMenuManager = __instance;
			});
		}
		if (LethalMenu.localPlayer == null)
		{
			AddToImportantObjectQueue(() =>
			{
				LethalMenu.localPlayer = GameNetworkManager.Instance?.localPlayerController;
			});
		}
	}

	[HarmonyPatch(typeof(PlayerControllerB), "Awake")]
	[HarmonyPostfix]
	private static void PlayerControllerBAwake(PlayerControllerB __instance)
	{
		if (!LethalMenu.players.Contains(__instance))
		{
			AddToImportantObjectQueue(() =>
			{
				LethalMenu.players.Add(__instance);
			});
		}
		if (!(LethalMenu.localPlayer == null))
		{
			return;
		}
		AddToImportantObjectQueue(() =>
		{
			LethalMenu.localPlayer = LethalMenu.players.Where((PlayerControllerB p) => p.IsLocalPlayer).FirstOrDefault();
		});
	}

	[HarmonyPatch(typeof(TerminalAccessibleObject), "Start")]
	[HarmonyPostfix]
	private static void TerminalAccessibleObjectStart(TerminalAccessibleObject __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.allTerminalObjects.Add(__instance);
		});
		if (__instance.isBigDoor)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.bigDoors.Add(__instance);
			});
		}
	}

	[HarmonyPatch(typeof(BreakerBox), "Start")]
	[HarmonyPostfix]
	private static void BreakerBoxStart(BreakerBox __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.breaker = __instance;
		});
	}

	[HarmonyPatch(typeof(Terminal), "Start")]
	[HarmonyPostfix]
	private static void TerminalStart(Terminal __instance)
	{
		AddToImportantObjectQueue(() =>
		{
			LethalMenu.terminal = __instance;
		});
	}

	[HarmonyPatch(typeof(GrabbableObject), "Start")]
	[HarmonyPostfix]
	private static void GrabbableObjectStart(GrabbableObject __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.items.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(Turret), "Start")]
	[HarmonyPostfix]
	private static void TurretStart(Turret __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.turrets.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(Landmine), "Start")]
	[HarmonyPostfix]
	private static void LandmineStart(Landmine __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.landmines.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(DoorLock), "Awake")]
	[HarmonyPostfix]
	private static void DoorLockAwake(DoorLock __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.doorLocks.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(EnemyAI), "Start")]
	[HarmonyPostfix]
	private static void EnemyAIStart(EnemyAI __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.enemies.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(MaskedPlayerEnemy), "Start")]
	[HarmonyPostfix]
	private static void MaskedPlayerEnemyStart(MaskedPlayerEnemy __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.enemies.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(SteamValveHazard), "Start")]
	[HarmonyPostfix]
	private static void SteamValveHazardStart(SteamValveHazard __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.steamValves.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(ShipTeleporter), "Awake")]
	[HarmonyPostfix]
	private static void ShipTeleporterAwake(ShipTeleporter __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.teleporters.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(InteractTrigger), "Start")]
	[HarmonyPostfix]
	private static void InteractTriggerStart(InteractTrigger __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.interactTriggers.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(SpikeRoofTrap), "Start")]
	[HarmonyPostfix]
	private static void SpikeRoofTrapStart(SpikeRoofTrap __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.spikeRoofTraps.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(AnimatedObjectTrigger), "Start")]
	[HarmonyPostfix]
	private static void AnimatedObjectTriggerStart(AnimatedObjectTrigger __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.animatedTriggers.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(EnemyVent), "Start")]
	[HarmonyPostfix]
	private static void EnemyVentStart(EnemyVent __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.enemyVents.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(MoldSpreadManager), "GenerateMold")]
	[HarmonyPostfix]
	private static void GenerateMold(MoldSpreadManager __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.vainShrouds = __instance.generatedMold;
		});
	}

	[HarmonyPatch(typeof(HangarShipDoor), "Start")]
	[HarmonyPostfix]
	private static void HangarShipDoorStart(HangarShipDoor __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.shipDoor = __instance;
		});
	}

	[HarmonyPatch(typeof(DepositItemsDesk), "Start")]
	[HarmonyPostfix]
	private static void DepositItemsDeskStart(DepositItemsDesk __instance)
	{
		AddToImportantObjectQueue(() =>
		{
			LethalMenu.depositItemsDesk = __instance;
		});
	}

	[HarmonyPatch(typeof(MineshaftElevatorController), "OnEnable")]
	[HarmonyPostfix]
	private static void MineshaftElevatorControllerOnEnable(MineshaftElevatorController __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.mineshaftElevator = __instance;
		});
	}

	[HarmonyPatch(typeof(ItemDropship), "Start")]
	[HarmonyPostfix]
	private static void ItemDropshipStart(ItemDropship __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.itemDropship = __instance;
		});
	}

	[HarmonyPatch(typeof(VehicleController), "Awake")]
	[HarmonyPostfix]
	private static void VehicleControllerAwake(VehicleController __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.vehicles.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(EntranceTeleport), "Awake")]
	[HarmonyPostfix]
	private static void EntranceTeleportAwake(EntranceTeleport __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.doors.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(LocalVolumetricFog), "Awake")]
	[HarmonyPostfix]
	private static void LocalVolumetricFogAwake(LocalVolumetricFog __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.fogs.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(Volume), "OnEnable")]
	[HarmonyPostfix]
	private static void VolumeOnEnable(Volume __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.volumes.Add(__instance);
		});
	}

	[HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
	[HarmonyPostfix]
	private static void OnDestroy(EnemyAI __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.enemies.Remove(__instance);
		});
	}

	[HarmonyPatch(typeof(PlayerControllerB), "OnDestroy")]
	[HarmonyPostfix]
	private static void OnDestroy(PlayerControllerB __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.players.Remove(__instance);
		});
	}

	[HarmonyPatch(typeof(TerminalAccessibleObject), "OnDestroy")]
	[HarmonyPostfix]
	private static void OnDestroy(TerminalAccessibleObject __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.allTerminalObjects.Remove(__instance);
		});
	}

	[HarmonyPatch(typeof(MoldSpreadManager), "RemoveAllMold")]
	[HarmonyPostfix]
	private static void RemoveAllMold(MoldSpreadManager __instance)
	{
		AddToObjectQueue(() =>
		{
			LethalMenu.vainShrouds = null;
		});
	}

	[HarmonyPatch(typeof(NetworkBehaviour), "OnDestroy")]
	[HarmonyPostfix]
	private static void OnDestroy(NetworkBehaviour __instance)
	{
		GrabbableObject GrabbableObject = __instance as GrabbableObject;
		if ((object)GrabbableObject != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.items.Remove(GrabbableObject);
			});
		}
		SteamValveFixInteraction SteamValveFixInteraction = __instance as SteamValveFixInteraction;
		if ((object)SteamValveFixInteraction != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.steamValves.Remove(SteamValveFixInteraction.steamValveMain);
			});
		}
		VehicleController VehicleController = __instance as VehicleController;
		if ((object)VehicleController != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.vehicles.Remove(VehicleController);
			});
		}
		Landmine Landmine = __instance as Landmine;
		if ((object)Landmine != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.landmines.Remove(Landmine);
			});
		}
		EntranceTeleport EntranceTeleport = __instance as EntranceTeleport;
		if ((object)EntranceTeleport != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.doors.Remove(EntranceTeleport);
			});
		}
		AnimatedObjectTrigger AnimatedObjectTrigger = __instance as AnimatedObjectTrigger;
		if ((object)AnimatedObjectTrigger != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.animatedTriggers.Remove(AnimatedObjectTrigger);
			});
		}
		InteractTrigger InteractTrigger = __instance as InteractTrigger;
		if ((object)InteractTrigger != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.interactTriggers.Remove(InteractTrigger);
			});
		}
		Turret Turret = __instance as Turret;
		if ((object)Turret != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.turrets.Remove(Turret);
			});
		}
		SpikeRoofTrap SpikeRoofTrap = __instance as SpikeRoofTrap;
		if ((object)SpikeRoofTrap != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.spikeRoofTraps.Remove(SpikeRoofTrap);
			});
		}
		EnemyVent EnemyVent = __instance as EnemyVent;
		if ((object)EnemyVent != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.enemyVents.Remove(EnemyVent);
			});
		}
		ShipTeleporter ShipTeleporter = __instance as ShipTeleporter;
		if ((object)ShipTeleporter != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.teleporters.Remove(ShipTeleporter);
			});
		}
		DoorLock DoorLock = __instance as DoorLock;
		if ((object)DoorLock != null)
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.doorLocks.Remove(DoorLock);
			});
		}
		if (__instance is BreakerBox)
		{
			AddToObjectQueue(() =>
			{
				AddToObjectQueue(() =>
				{
					LethalMenu.breaker = null;
				});
			});
		}
		if (__instance is Terminal)
		{
			AddToObjectQueue(() =>
			{
				AddToObjectQueue(() =>
				{
					LethalMenu.terminal = null;
				});
			});
		}
		if (__instance is ItemDropship)
		{
			AddToObjectQueue(() =>
			{
				AddToObjectQueue(() =>
				{
					LethalMenu.itemDropship = null;
				});
			});
		}
		if (__instance is MineshaftElevatorController)
		{
			AddToObjectQueue(() =>
			{
				AddToObjectQueue(() =>
				{
					LethalMenu.mineshaftElevator = null;
				});
			});
		}
		if (!(__instance is DepositItemsDesk))
		{
			return;
		}
		AddToObjectQueue(() =>
		{
			AddToObjectQueue(() =>
			{
				LethalMenu.depositItemsDesk = null;
			});
		});
	}

	public static void CollectObjects()
	{
		CollectObjects(LethalMenu.items);
		CollectObjects(LethalMenu.landmines);
		CollectObjects(LethalMenu.turrets);
		CollectObjects(LethalMenu.doors);
		CollectObjects(LethalMenu.players);
		CollectObjects(LethalMenu.enemies);
		CollectObjects(LethalMenu.steamValves);
		CollectObjects(LethalMenu.allTerminalObjects);
		CollectObjects(LethalMenu.teleporters);
		CollectObjects(LethalMenu.interactTriggers);
		CollectObjects(LethalMenu.bigDoors, (TerminalAccessibleObject obj) => obj.isBigDoor);
		CollectObjects(LethalMenu.doorLocks);
		CollectObjects(LethalMenu.spikeRoofTraps);
		CollectObjects(LethalMenu.animatedTriggers);
		CollectObjects(LethalMenu.vehicles);
		CollectObjects(LethalMenu.enemyVents);
		CollectObjects(LethalMenu.fogs);
		CollectObjects(LethalMenu.volumes);
		LethalMenu.localPlayer = GameNetworkManager.Instance?.localPlayerController;
		LethalMenu.itemDropship = UnityEngine.Object.FindAnyObjectByType<ItemDropship>();
		LethalMenu.breaker = UnityEngine.Object.FindAnyObjectByType<BreakerBox>();
		LethalMenu.shipDoor = UnityEngine.Object.FindAnyObjectByType<HangarShipDoor>();
		LethalMenu.mineshaftElevator = UnityEngine.Object.FindAnyObjectByType<MineshaftElevatorController>();
		LethalMenu.vainShrouds = UnityEngine.Object.FindAnyObjectByType<MoldSpreadManager>()?.generatedMold;
		LethalMenu.quickMenuManager = UnityEngine.Object.FindAnyObjectByType<QuickMenuManager>();
		LethalMenu.terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();
		LethalMenu.depositItemsDesk = UnityEngine.Object.FindAnyObjectByType<DepositItemsDesk>();
	}

	private static void CollectObjects<T>(List<T> list, Func<T, bool> filter = null) where T : MonoBehaviour
	{
		IEnumerable<T> collection;
		if (filter != null)
		{
			collection = UnityEngine.Object.FindObjectsOfType<T>().Where(filter);
		}
		else
		{
			IEnumerable<T> enumerable = UnityEngine.Object.FindObjectsOfType<T>();
			collection = enumerable;
		}
		list.AddRange(collection);
	}

	public static void ClearObjects()
	{
		LethalMenu.items?.Clear();
		LethalMenu.landmines?.Clear();
		LethalMenu.turrets?.Clear();
		LethalMenu.doors?.Clear();
		LethalMenu.players?.Clear();
		LethalMenu.enemies?.Clear();
		LethalMenu.steamValves?.Clear();
		LethalMenu.allTerminalObjects?.Clear();
		LethalMenu.teleporters?.Clear();
		LethalMenu.interactTriggers?.Clear();
		LethalMenu.bigDoors?.Clear();
		LethalMenu.doorLocks?.Clear();
		LethalMenu.spikeRoofTraps?.Clear();
		LethalMenu.animatedTriggers?.Clear();
		LethalMenu.enemyVents?.Clear();
		LethalMenu.vainShrouds?.Clear();
		LethalMenu.vehicles?.Clear();
		LethalMenu.fogs?.Clear();
		LethalMenu.shipDoor = null;
		LethalMenu.breaker = null;
		LethalMenu.mineshaftElevator = null;
		LethalMenu.itemDropship = null;
		LethalMenu.localPlayer = null;
		LethalMenu.quickMenuManager = null;
		LethalMenu.terminal = null;
		LethalMenu.depositItemsDesk = null;
	}

	public static void AddToImportantObjectQueue(Action action)
	{
		ObjectQueue.Enqueue(action);
		if (!ImportantCoroutineStarted)
		{
			LethalMenu.Instance.StartCoroutine(RunImportantObjectQueue());
		}
	}

	public static void AddToObjectQueue(Action action)
	{
		ObjectQueue.Enqueue(action);
		if (!CoroutineStarted)
		{
			LethalMenu.Instance.StartCoroutine(RunObjectQueue());
		}
	}

	private static IEnumerator RunImportantObjectQueue()
	{
		ImportantCoroutineStarted = true;
		while (ImportantObjectQueue.Count > 0)
		{
			ImportantObjectQueue.Dequeue()?.Invoke();
			yield return null;
		}
		ImportantCoroutineStarted = false;
	}

	private static IEnumerator RunObjectQueue()
	{
		CoroutineStarted = true;
		while (ObjectQueue.Count > 0)
		{
			yield return new WaitForSeconds(Settings.f_ObjectQueueDelay * 0.1f);
			ObjectQueue.Dequeue()?.Invoke();
		}
		CoroutineStarted = false;
	}
}
