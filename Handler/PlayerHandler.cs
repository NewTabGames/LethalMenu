using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using UnityEngine;

namespace LethalMenu.Handler;

public class PlayerHandler
{
	private PlayerControllerB player;

	public PlayerHandler(PlayerControllerB player)
	{
		this.player = player;
	}

	public void Teleport(Vector3 pos, bool elevator = false, bool ship = false, bool factory = false)
	{
		player.TeleportPlayer(pos);
		player.isInElevator = elevator;
		player.isInHangarShipRoom = ship;
		player.isInsideFactory = factory;
		HUDManager.Instance.DisplayTip("Lethal Menu", "Teleported");
	}

	public void SavePosition()
	{
		Settings.v_savedLocation = player.transform.position;
		HUDManager.Instance.DisplayTip("Lethal Menu", "Teleport Position Saved");
	}

	public void TeleportShip()
	{
		Teleport(StartOfRound.Instance ? StartOfRound.Instance.playerSpawnPositions[0].transform.position : Vector3.zero, elevator: false, ship: true);
	}

	public void TeleportSaved()
	{
		if (Settings.v_savedLocation == Vector3.zero)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "No Saved Position", isWarning: true);
		}
		else
		{
			Teleport(Settings.v_savedLocation);
		}
	}

	public void TeleportTo()
	{
		LethalMenu.localPlayer.Handle().Teleport(player.transform.position, player.isInElevator, player.isInHangarShipRoom, player.isInsideFactory);
	}

	/// <summary>
	/// The player's slot number in the lobby. Sent as "who hit them": the game looks that player up whenever
	/// the target ends below 6 HP, and the old value -1 crashed that step on every player's game.
	/// Using the target themselves keeps it pointing away from you.
	/// </summary>
	private int SlotNumber()
	{
		PlayerControllerB[] all = StartOfRound.Instance != null ? StartOfRound.Instance.allPlayerScripts : null;
		int index = all != null ? System.Array.IndexOf(all, player) : -1;
		return index >= 0 ? index : (int)player.playerClientId;
	}

	/// <summary>
	/// Uses the game's "another player hit them" message, which anyone may send. The damage is at least 100:
	/// the game lets a hit under 50 that would kill someone who isn't badly hurt leave them on 5 HP instead.
	/// </summary>
	public void Kill()
	{
		if (player == null || player.isPlayerDead)
		{
			return;
		}
		player.DamagePlayerFromOtherClientServerRpc(Mathf.Max(100, player.health + 1), Vector3.zero, SlotNumber());
	}

	/// <summary>
	/// Full health. Each player's own game is in charge of their health, so:
	/// - yourself: set it here and tell everyone (quiet, also stops the limp and bleeding);
	/// - anyone else, host or not: the "another player hit them" message with -100 damage, which their own
	///   game turns into +100 HP (they hear a hit sound). The host can't set it directly: the game's direct
	///   health message only accepts the player's own game, so the old host-only route did nothing to others.
	/// </summary>
	public void Heal()
	{
		if (player == null || player.isPlayerDead)
		{
			return;
		}
		if (player == LethalMenu.localPlayer)
		{
			player.health = 100;
			if (player.criticallyInjured)
			{
				player.MakeCriticallyInjured(enable: false);
			}
			if (HUDManager.Instance != null)
			{
				HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
				HUDManager.Instance.SetCracksOnVisor(100f);
			}
			player.DamagePlayerServerRpc(0, 100); // allowed: it's your own player
			return;
		}
		player.DamagePlayerFromOtherClientServerRpc(-100, Vector3.zero, SlotNumber());
	}

	/// <summary>
	/// The game's targeted lightning only works while the stormy-weather effect is running (stormy moon, landed);
	/// otherwise every client throws inside StormyWeather.LightningStrike.
	/// </summary>
	public static bool CanStrike()
	{
		return Object.FindObjectOfType<StormyWeather>() != null;
	}

	public void Strike()
	{
		if (!CanStrike())
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Lightning only works on a stormy moon after landing");
			return;
		}
		RoundManager.Instance?.LightningStrikeServerRpc(player.transform.position);
	}

	public void Spectate()
	{
		SpectatePlayer.spectatingPlayer = (int)player.playerClientId;
	}

	public void MiniCam()
	{
		SpectatePlayer.camPlayer = (int)player.playerClientId;
	}

	public void SpawnSpiderWebs(int count = 1)
	{
		SandSpiderAI spider = SandSpiderAIExtensions.FindSpider();
		if (spider == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Spider webs need a living Bunker Spider somewhere on the map");
			return;
		}
		spider.SpawnWebs(player.transform.position, count);
	}

	/// <summary>Nearest landmine that still exists and hasn't gone off, or null.</summary>
	public static Landmine ClosestLandmine(PlayerControllerB player)
	{
		if (player == null || LethalMenu.landmines == null)
		{
			return null;
		}
		return LethalMenu.landmines.Where((Landmine m) => m != null && !m.hasExploded).OrderBy((Landmine m) => Vector3.Distance(m.transform.position, player.transform.position)).FirstOrDefault();
	}

	public void ExplodeClosestLandmine()
	{
		Landmine landmine = ClosestLandmine(player);
		if (landmine == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "There are no live landmines on this moon");
			return;
		}
		landmine.ExplodeMineServerRpc();
	}

	public void LureAllEnemies()
	{
		LethalMenu.enemies.FindAll((EnemyAI e) => e != null && !e.isEnemyDead).ForEach((EnemyAI e) =>
		{
			// One monster failing must not stop the rest from being lured.
			try
			{
				e.Handle().TargetPlayer(player);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Lure failed for " + e.name + ": " + ex.Message);
			}
		});
	}

	public void SellQuota()
	{
		int quotaLeft = TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;
		List<GrabbableObject> list = LethalMenu.items.Where((GrabbableObject i) => i != null && i.isInShipRoom && !i.isHeld && !i.isPocketed && i.itemProperties.isScrap && !i.IsDefaultShipItem()).ToList();
		list.ForEach((GrabbableObject i) =>
		{
			if (quotaLeft >= 0)
			{
				Patches.SellQuota = true;
				quotaLeft -= (int)((float)i.scrapValue * StartOfRound.Instance.companyBuyingRate);
				i.PlaceOnDesk();
			}
		});
	}

	public void PlaceEverythingOnDesk()
	{
		player.DropAllHeldItems(true, false);
		LethalMenu.items.Where((GrabbableObject i) => i.itemProperties.isScrap && !i.isHeld && !i.isPocketed && i.itemProperties.isScrap && !i.IsDefaultShipItem()).ToList().ForEach((GrabbableObject i) =>
		{
			i.PlaceOnDesk();
		});
	}

	public bool HasLineOfSight(Component o)
	{
		return player != null && o != null && player.HasLineOfSightToPosition(o.transform.position, 45f, 60, -1f);
	}

	public PlayerHandler GetHandler(PlayerControllerB player)
	{
		return new PlayerHandler(player);
	}
}
