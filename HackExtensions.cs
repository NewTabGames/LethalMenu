using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Language;
using LethalMenu.Manager;
using LethalMenu.Types;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LethalMenu;

public static class HackExtensions
{
	private static readonly Hack[] KeyBindIgnore = new Hack[]
	{
		Hack.ModifyQuota,
		Hack.ModifyCredits,
		Hack.KillPlayer,
		Hack.HealPlayer,
		Hack.LightningStrikePlayer,
		Hack.Experience,
		Hack.Teleport,
		Hack.DeathNotify,
		Hack.SpectatePlayer,
		Hack.MiniCam,
		Hack.ModifyScrap,
		Hack.ModifyDeadline,
		Hack.Message,
		Hack.SpiderWebPlayer,
		Hack.TeleportAllEnemies,
		Hack.TeleportEnemy,
		Hack.EnemyControl,
		Hack.LureAllEnemies,
		Hack.ExplodeClosestMine,
		Hack.SpawnMapObjects,
		Hack.ExplodeJetpack,
		Hack.HuntPlayer,
		Hack.PersistentHunt,
		Hack.AggroHunt,
		Hack.HuntWithAll
	};

	private static List<Hack> Waiting = new List<Hack>();

	public static readonly Dictionary<Hack, bool> ToggleFlags = new Dictionary<Hack, bool>
	{
		{
			Hack.OpenMenu,
			false
		},
		{
			Hack.ToggleCursor,
			false
		},
		{
			Hack.GodMode,
			false
		},
		{
			Hack.SuperJump,
			false
		},
		{
			Hack.FastClimb,
			false
		},
		{
			Hack.NightVision,
			false
		},
		{
			Hack.UnlimitedStamina,
			false
		},
		{
			Hack.UnlimitedBattery,
			false
		},
		{
			Hack.UnlimitedOxygen,
			false
		},
		{
			Hack.LootThroughWalls,
			false
		},
		{
			Hack.InteractThroughWalls,
			false
		},
		{
			Hack.Reach,
			false
		},
		{
			Hack.UnlockObjects,
			false
		},
		{
			Hack.AlwaysShowClock,
			false
		},
		{
			Hack.SimpleClock,
			false
		},
		{
			Hack.SuperSpeed,
			false
		},
		{
			Hack.NoClip,
			false
		},
		{
			Hack.NameESP,
			false
		},
		{
			Hack.BoxESP,
			false
		},
		{
			Hack.ObjectESP,
			false
		},
		{
			Hack.EnemyESP,
			false
		},
		{
			Hack.PlayerESP,
			false
		},
		{
			Hack.BodyESP,
			false
		},
		{
			Hack.DoorESP,
			false
		},
		{
			Hack.LandmineESP,
			false
		},
		{
			Hack.TurretESP,
			false
		},
		{
			Hack.ShipESP,
			false
		},
		{
			Hack.BreakerESP,
			false
		},
		{
			Hack.SteamHazardESP,
			false
		},
		{
			Hack.BigDoorESP,
			false
		},
		{
			Hack.DoorLockESP,
			false
		},
		{
			Hack.SpikeRoofTrapESP,
			false
		},
		{
			Hack.EnemyVentESP,
			false
		},
		{
			Hack.VainShroudESP,
			false
		},
		{
			Hack.CruiserESP,
			false
		},
		{
			Hack.ItemDropShipESP,
			false
		},
		{
			Hack.Crosshair,
			false
		},
		{
			Hack.DisplayBodies,
			false
		},
		{
			Hack.DisplayEnemies,
			false
		},
		{
			Hack.DisplayMapObjects,
			false
		},
		{
			Hack.DisplayShipObjects,
			false
		},
		{
			Hack.DisplayQuota,
			false
		},
		{
			Hack.DisplayDeadline,
			false
		},
		{
			Hack.DisplayCredits,
			false
		},
		{
			Hack.ToggleAllLandmines,
			false
		},
		{
			Hack.ToggleAllTurrets,
			false
		},
		{
			Hack.ToggleShipHorn,
			false
		},
		{
			Hack.ToggleCarHorn,
			false
		},
		{
			Hack.Breadcrumbs,
			false
		},
		{
			Hack.DeathNotifications,
			false
		},
		{
			Hack.EnemyDeathNotifications,
			false
		},
		{
			Hack.BuildAnywhere,
			false
		},
		{
			Hack.Weight,
			false
		},
		{
			Hack.GhostMode,
			false
		},
		{
			Hack.UnlimitedAmmo,
			false
		},
		{
			Hack.FreeCam,
			false
		},
		{
			Hack.SpectatePlayer,
			false
		},
		{
			Hack.MiniCam,
			false
		},
		{
			Hack.EnemyControl,
			false
		},
		{
			Hack.NoCooldown,
			false
		},
		{
			Hack.InstantInteract,
			false
		},
		{
			Hack.SuperShovel,
			false
		},
		{
			Hack.StrongHands,
			false
		},
		{
			Hack.Invisibility,
			false
		},
		{
			Hack.NoFallDamage,
			false
		},
		{
			Hack.Shoplifter,
			false
		},
		{
			Hack.HearAllAlivePeople,
			false
		},
		{
			Hack.HearAllDeadPeople,
			false
		},
		{
			Hack.NoVisor,
			false
		},
		{
			Hack.NoFieldOfDepth,
			false
		},
		{
			Hack.NoFlash,
			false
		},
		{
			Hack.TeleportWithItems,
			false
		},
		{
			Hack.OpenShipDoorSpace,
			false
		},
		{
			Hack.UnlimitedPresents,
			false
		},
		{
			Hack.ShowOffensiveLobbyNames,
			false
		},
		{
			Hack.NoCameraShake,
			false
		},
		{
			Hack.SuperKnife,
			false
		},
		{
			Hack.NoQuicksand,
			false
		},
		{
			Hack.BerserkAllTurrets,
			false
		},
		{
			Hack.BridgeNeverFalls,
			false
		},
		{
			Hack.VehicleGodMode,
			false
		},
		{
			Hack.EggsNeverExplode,
			false
		},
		{
			Hack.UnlimitedZapGun,
			false
		},
		{
			Hack.ToggleTerminalSound,
			false
		},
		{
			Hack.LootBeforeGameStarts,
			false
		},
		{
			Hack.ClickTeleport,
			false
		},
		{
			Hack.ClickKill,
			false
		},
		{
			Hack.PJSpammer,
			false
		},
		{
			Hack.ItemSlots,
			false
		},
		{
			Hack.FOV,
			false
		},
		{
			Hack.AntiGhostGirl,
			false
		},
		{
			Hack.EggsAlwaysExplode,
			false
		},
		{
			Hack.NoShipDoorClose,
			false
		},
		{
			Hack.OpenDropShipLand,
			false
		},
		{
			Hack.LootAnyItemBeltBag,
			false
		},
		{
			Hack.LootThroughWallsBeltBag,
			false
		},
		{
			Hack.UnlimitedTZP,
			false
		},
		{
			Hack.NoTZPEffects,
			false
		},
		{
			Hack.HPDisplay,
			false
		},
		{
			Hack.MineshaftElevatorESP,
			false
		},
		{
			Hack.AntiKick,
			false
		},
		{
			Hack.DemiGod,
			false
		},
		{
			Hack.FullRenderResolution,
			false
		},
		{
			Hack.ToggleDepositDeskDoorSound,
			false
		},
		{
			Hack.GrabNutcrackerShotgun,
			false
		},
		{
			Hack.MinigunShotgun,
			false
		},
		{
			Hack.SpamShootAllShotguns,
			false
		},
		{
			Hack.SlideTaunt,
			false
		},
		{
			Hack.ExplodeJetpacksOnGrab,
			false
		}
	};

	private static readonly Dictionary<Hack, Delegate> Executors = new Dictionary<Hack, Delegate>
	{
		{
			Hack.ToggleCursor,
			new Action(HackExecutor.ToggleCursor)
		},
		{
			Hack.OpenMenu,
			new Action(HackExecutor.OpenMenu)
		},
		{
			Hack.UnloadMenu,
			new Action(HackExecutor.UnloadMenu)
		},
		{
			Hack.Experience,
			new Action<int, ActionType>(HackExecutor.ModExperience)
		},
		{
			Hack.Teleport,
			new Action<Vector3, bool, bool, bool>(HackExecutor.Teleport)
		},
		{
			Hack.TeleportShip,
			new Action(HackExecutor.TeleportShip)
		},
		{
			Hack.TeleportSavedPosition,
			new Action(HackExecutor.TeleportSavedPosition)
		},
		{
			Hack.SaveTeleportPosition,
			new Action(HackExecutor.SaveTeleportPosition)
		},
		{
			Hack.ModifyCredits,
			new Action<int, ActionType>(HackExecutor.ModCredits)
		},
		{
			Hack.ModifyQuota,
			new Action<int>(HackExecutor.ModQuota)
		},
		{
			Hack.ModifyDeadline,
			new Action<int>(HackExecutor.ModDeadline)
		},
		{
			Hack.Message,
			new Action<string, int, int>(HackExecutor.Message)
		},
		{
			Hack.SpawnMoreScrap,
			new Action(HackExecutor.SpawnMoreScrap)
		},
		{
			Hack.EndGame,
			new Action(HackExecutor.EndGame)
		},
		{
			Hack.StartGame,
			new Action(HackExecutor.StartGame)
		},
		{
			Hack.ToggleShipLights,
			new Action(HackExecutor.ToggleShipLights)
		},
		{
			Hack.ToggleCarHorn,
			new Action(HackExecutor.ToggleCarHorn)
		},
		{
			Hack.ToggleFactoryLights,
			new Action(HackExecutor.ToggleFactoryLights)
		},
		{
			Hack.ToggleShipHorn,
			new Action(HackExecutor.ToggleShipHorn)
		},
		{
			Hack.ForceBridgeFall,
			new Action(HackExecutor.ForceBridgeFall)
		},
		{
			Hack.ForceSmallBridgeFall,
			new Action(HackExecutor.ForceSmallBridgeFall)
		},
		{
			Hack.BlowUpAllLandmines,
			new Action(HackExecutor.BlowUpAllLandmines)
		},
		{
			Hack.KillAllEnemies,
			new Action(HackExecutor.KillAllEnemies)
		},
		{
			Hack.KillNearbyEnemies,
			new Action<int>(HackExecutor.KillNearbyEnemies)
		},
		{
			Hack.StunAllEnemies,
			new Action(HackExecutor.StunAllEnemies)
		},
		{
			Hack.ForceTentacleAttack,
			new Action(HackExecutor.ForceTentacleAttack)
		},
		{
			Hack.ToggleAllLandmines,
			new Action(HackExecutor.ToggleAllLandmines)
		},
		{
			Hack.ToggleAllTurrets,
			new Action(HackExecutor.ToggleAllTurrets)
		},
		{
			Hack.KillPlayer,
			new Action<PlayerControllerB>(HackExecutor.KillPlayer)
		},
		{
			Hack.HealPlayer,
			new Action<PlayerControllerB>(HackExecutor.HealPlayer)
		},
		{
			Hack.DemiGod,
			new Action<PlayerControllerB>(HackExecutor.DemiGod)
		},
		{
			Hack.LightningStrikePlayer,
			new Action<PlayerControllerB>(HackExecutor.LightningStrikePlayer)
		},
		{
			Hack.UnlockObjectsAction,
			new Action(HackExecutor.UnlockObjects)
		},
		{
			Hack.ToggleAllESP,
			new Action(HackExecutor.ToggleAllESP)
		},
		{
			Hack.DeathNotify,
			new Action<PlayerControllerB, CauseOfDeath>(HackExecutor.NotifyDeath)
		},
		{
			Hack.EnemyDeathNotify,
			new Action<EnemyType>(HackExecutor.NotifyEnemyDeath)
		},
		{
			Hack.SpectatePlayer,
			new Action<PlayerControllerB>(HackExecutor.SpectatePlayer)
		},
		{
			Hack.MiniCam,
			new Action<PlayerControllerB>(HackExecutor.MiniCam)
		},
		{
			Hack.FixAllValves,
			new Action(HackExecutor.FixAllValves)
		},
		{
			Hack.ModifyScrap,
			new Action<int, int>(HackExecutor.ModScrap)
		},
		{
			Hack.TeleportAllEnemies,
			new Action<PlayerControllerB, EnemyAI[]>(HackExecutor.TeleportAllEnemies)
		},
		{
			Hack.TeleportEnemy,
			new Action<PlayerControllerB, EnemyAI>(HackExecutor.TeleportEnemy)
		},
		{
			Hack.EnemyControl,
			new Action<EnemyAI>(HackExecutor.ControlEnemy)
		},
		{
			Hack.HuntPlayer,
			new Action<EnemyAI, PlayerControllerB>(HackExecutor.HuntPlayer)
		},
		{
			Hack.StopAllHunts,
			new Action(HackExecutor.StopAllHunts)
		},
		{
			Hack.HuntWithAll,
			new Action<PlayerControllerB>(HackExecutor.HuntWithAll)
		},
		{
			Hack.PersistentHunt,
			new Action<EnemyAI, PlayerControllerB>(HackExecutor.PersistentHunt)
		},
		{
			Hack.AggroHunt,
			new Action<EnemyAI, PlayerControllerB>(HackExecutor.AggroHunt)
		},
		{
			Hack.SpawnMaskedEnemy,
			new Action(HackExecutor.SpawnMaskedEnemy)
		},
		{
			Hack.SpiderWebPlayer,
			new Action<PlayerControllerB>(HackExecutor.SpiderWebPlayer)
		},
		{
			Hack.BreakAllWebs,
			new Action(HackExecutor.BreakAllWebs)
		},
		{
			Hack.LureAllEnemies,
			new Action<PlayerControllerB>(HackExecutor.LureAllEnemies)
		},
		{
			Hack.SpawnMapObjects,
			new Action<MapObject>(HackExecutor.SpawnMapObjects)
		},
		{
			Hack.SpawnLandmine,
			new Action(HackExecutor.SpawnLandmine)
		},
		{
			Hack.SpawnTurret,
			new Action(HackExecutor.SpawnTurret)
		},
		{
			Hack.SpawnSpikeRoofTrap,
			new Action(HackExecutor.SpawnSpikeRoofTrap)
		},
		{
			Hack.ExplodeClosestMine,
			new Action<PlayerControllerB>(HackExecutor.ExplodeClosestMine)
		},
		{
			Hack.SellQuota,
			new Action(HackExecutor.SellQuota)
		},
		{
			Hack.SellEverything,
			new Action(HackExecutor.SellEverything)
		},
		{
			Hack.TeleportAllItems,
			new Action(HackExecutor.TeleportAllItems)
		},
		{
			Hack.TeleportOneItem,
			new Action(HackExecutor.TeleportOneItem)
		},
		{
			Hack.EjectEveryone,
			new Action(HackExecutor.EjectEveryone)
		},
		{
			Hack.JoinLobby,
			new Action<SteamId>(HackExecutor.JoinLobby)
		},
		{
			Hack.Disconnect,
			new Action(HackExecutor.Disconnect)
		},
		{
			Hack.VoteShipLeaveEarly,
			new Action(HackExecutor.VoteShipLeaveEarly)
		},
		{
			Hack.UnlockAllDoors,
			new Action(HackExecutor.UnlockAllDoors)
		},
		{
			Hack.DropAllItems,
			new Action(HackExecutor.DropAllItems)
		},
		{
			Hack.BerserkAllTurrets,
			new Action(HackExecutor.BerserkAllTurrets)
		},
		{
			Hack.ResetShip,
			new Action(HackExecutor.ResetShip)
		},
		{
			Hack.ToggleAllDisplays,
			new Action(HackExecutor.ToggleAllDisplays)
		},
		{
			Hack.ToggleTerminalSound,
			new Action(HackExecutor.ToggleTerminalSound)
		},
		{
			Hack.DeleteHeldItem,
			new Action(HackExecutor.DeleteHeldItem)
		},
		{
			Hack.ClickTeleportAction,
			new Action(HackExecutor.ClickTeleport)
		},
		{
			Hack.ClickKillAction,
			new Action(HackExecutor.ClickKill)
		},
		{
			Hack.ForceMeteorShower,
			new Action(HackExecutor.ForceMeteorShower)
		},
		{
			Hack.ClearMeteorShower,
			new Action(HackExecutor.ClearMeteorShower)
		},
		{
			Hack.OpenAllBigDoors,
			new Action(HackExecutor.OpenAllBigDoors)
		},
		{
			Hack.CloseAllBigDoors,
			new Action(HackExecutor.CloseAllBigDoors)
		},
		{
			Hack.SpawnHoardingBugInfestation,
			new Action(HackExecutor.SpawnHoardingBugInfestation)
		},
		{
			Hack.ToggleMineshaftElevator,
			new Action(HackExecutor.ToggleMineshaftElevator)
		},
		{
			Hack.ToggleVehicleMagnet,
			new Action(HackExecutor.ToggleVehicleMagnet)
		},
		{
			Hack.SpamShootAllShotguns,
			new Action(HackExecutor.SpamShootAllShotguns)
		},
		{
			Hack.ShootAllShotguns,
			new Action(HackExecutor.ShootAllShotguns)
		},
		{
			Hack.ExplodeCruiser,
			new Action(HackExecutor.ExplodeCruiser)
		},
		{
			Hack.ExplodeJetpack,
			new Action<JetpackItem>(HackExecutor.ExplodeJetpack)
		},
		{
			Hack.ExplodeJetpacksOnGrab,
			new Action(HackExecutor.ExplodeJetpacksOnGrab)
		},
		{
			Hack.ExplodeAllJetpacks,
			new Action(HackExecutor.ExplodeAllJetpacks)
		}
	};

	public static readonly Dictionary<Hack, ButtonControl> KeyBinds = new Dictionary<Hack, ButtonControl>
	{
		{
			Hack.OpenMenu,
			Keyboard.current.insertKey
		},
		{
			Hack.ToggleCursor,
			Keyboard.current.altKey
		},
		{
			Hack.UnloadMenu,
			Keyboard.current.pauseKey
		},
		{
			Hack.UnlockObjectsAction,
			Keyboard.current.f1Key
		},
		{
			Hack.ClickTeleportAction,
			Mouse.current.middleButton
		},
		{
			Hack.ClickKillAction,
			Mouse.current.middleButton
		}
	};

	public static void Execute(this Hack hack, params object[] param)
	{
		if (hack.CanBeToggled())
		{
			hack.Toggle();
		}
		if (Executors.TryGetValue(hack, out var value))
		{
			if (value is Action action)
			{
				action();
			}
			else
			{
				value.DynamicInvoke(param);
			}
		}
	}

	public static void Invoke(this Hack hack, params object[] param)
	{
		if (Executors.TryGetValue(hack, out var value))
		{
			if (value is Action action)
			{
				action();
			}
			else
			{
				value.DynamicInvoke(param);
			}
		}
	}

	/// <summary>The delegate a hack runs when executed (null for pure toggles). Used by the remote panel to read parameter types.</summary>
	internal static Delegate GetExecutor(this Hack hack)
	{
		return Executors.TryGetValue(hack, out var value) ? value : null;
	}

	public static bool CanBeToggled(this Hack hack)
	{
		return ToggleFlags.ContainsKey(hack);
	}

	public static bool CanBeExecuted(this Hack hack)
	{
		return Executors.ContainsKey(hack);
	}

	public static bool IsEnabled(this Hack hack)
	{
		return ToggleFlags[hack];
	}

	public static void Toggle(this Hack hack)
	{
		ToggleFlags[hack] = !ToggleFlags[hack];
	}

	public static void SetToggle(this Hack hack, bool b)
	{
		ToggleFlags[hack] = b;
	}

	public static void RemoveKeyBind(this Hack hack)
	{
		if (KeyBinds.ContainsKey(hack))
		{
			KeyBinds.Remove(hack);
		}
	}

	public static void SetKeyBind(this Hack hack, ButtonControl btn)
	{
		if (!Enumerable.Contains(KeyBindIgnore, hack) && btn != null)
		{
			if (KeyBinds.ContainsKey(hack))
			{
				KeyBinds[hack] = btn;
			}
			else
			{
				KeyBinds.Add(hack, btn);
			}
		}
	}

	public static bool Button(this Hack hack)
	{
		return GUILayout.Button(Localization.Localize(hack.ToBtnText()));
	}

	public static string ToBtnText(this Hack hack)
	{
		string result;
		if (hack.CanBeToggled())
		{
			result = (ToggleFlags[hack] ? "General.Disable" : "General.Enable");
		}
		else
		{
			result = "General.Execute";
		}
		return result;
	}

	public static ButtonControl GetKeyBind(this Hack hack)
	{
		return KeyBinds.ContainsKey(hack) ? KeyBinds[hack] : null;
	}

	public static bool HasKeyBind(this Hack hack)
	{
		return KeyBinds.ContainsKey(hack);
	}

	public static bool CanHaveKeyBind(this Hack hack)
	{
		return !Enumerable.Contains(KeyBindIgnore, hack);
	}

	public static bool IsWaiting(this Hack hack)
	{
		return Waiting.Contains(hack);
	}

	public static bool IsAnyHackWaiting(this Hack hack)
	{
		return Waiting.Count > 0;
	}

	public static void SetWaiting(this Hack hack, bool b)
	{
		if (b && !Waiting.Contains(hack))
		{
			Waiting.Add(hack);
		}
		if (!b && Waiting.Contains(hack))
		{
			Waiting.Remove(hack);
		}
	}

	public static bool KeyBindInUse(ButtonControl btn)
	{
		foreach (Hack key in KeyBinds.Keys)
		{
			if (KeyBinds[key] == btn)
			{
				return true;
			}
		}
		return false;
	}
}
