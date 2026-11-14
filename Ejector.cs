using System;
using System.Collections;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Components;
using LethalMenu.Handler;
using LethalMenu.Themes;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu;

/// <summary>
/// Cleanly removes the menu from the running game: turns every hack off, gives the cheats a few frames to
/// restore the game's own values, undoes the things that do not restore themselves, removes every Harmony
/// patch and destroys the menu object. Afterwards the game behaves as if the menu was never loaded.
/// </summary>
internal static class Ejector
{
	/// <summary>Frames the cheats get to put the game's values back after everything is switched off.</summary>
	private const int RestoreFrames = 3;

	internal static bool Ejecting { get; private set; }

	public static void Eject()
	{
		if (Ejecting || !Loader.IsLoaded)
		{
			return;
		}
		LethalMenu menu = LethalMenu.Current;
		if (menu == null)
		{
			Finish();
			return;
		}
		Ejecting = true;
		menu.StartCoroutine(EjectRoutine());
	}

	private static IEnumerator EjectRoutine()
	{
		Debug.Log("[LethalMenu] Ejecting...");
		Step("disconnect remote panel", Remote.RemoteControl.Stop);
		Step("release controlled enemy", EnemyControl.StopControl);
		Step("stop monster hunts", () => EnemyHunt.StopAll());
		Step("stop error capture", Util.DebugLog.Detach);
		Step("stop freecam", Freecam.Reset);
		Step("disable hacks", () =>
		{
			HackExtensions.ToggleFlags.Keys.ToList().ForEach((Hack h) =>
			{
				HackExtensions.ToggleFlags[h] = false;
			});
		});

		// Most cheats write the game's default back when their toggle is off (speed, climb speed, reach, FOV,
		// night vision, visor, noclip collider, HP text, shop prices, spectate camera...) and the patched
		// LateUpdate/Update postfixes do the same for grab distance, interact mask and carry weight.
		for (int i = 0; i < RestoreFrames; i++)
		{
			yield return null;
		}

		Step("spectate cleanup", SpectatePlayer.Reset);
		Step("player values", RestorePlayer);
		Step("render resolution", FullRenderResolution.Restore);
		Step("chams", ChamHandler.RestoreAll);
		Step("fog", NoFog.RestoreAll);
		Step("interact timings", NoCooldownInteractions.RestoreAll);
		Step("harmony patches", () =>
		{
			LethalMenu.harmony?.UnpatchAll(LethalMenu.harmony.Id);
			LethalMenu.harmony = null;
		});
		Step("input and cursor", RestoreInput);
		Step("theme", Theme.Unload);
		Step("state", ClearState);
		Finish();
	}

	private static void Finish()
	{
		Ejecting = false;
		Loader.DestroyHost();
		string reload = Loader.RunningUnderBepInEx && Plugin.ReloadKey != null && Plugin.ReloadKey.Value != UnityEngine.InputSystem.Key.None ? " Press " + Plugin.ReloadKey.Value + " to load it again." : "";
		Debug.Log("[LethalMenu] Ejected." + reload);
		try
		{
			if (HUDManager.Instance != null && StartOfRound.Instance != null && GameNetworkManager.Instance?.localPlayerController != null)
			{
				HUDManager.Instance.DisplayTip("Lethal Menu", "Ejected." + reload);
			}
		}
		catch
		{
		}
	}

	private static void Step(string name, Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Eject step '" + name + "' failed: " + ex.Message);
		}
	}

	private static void RestorePlayer()
	{
		PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
		if (player == null)
		{
			return;
		}
		// SuperJump only rewrites jumpForce when the player jumps, so put it back now.
		if (Mathf.Approximately(player.jumpForce, Settings.f_jumpForce) && !Mathf.Approximately(Settings.f_jumpForce, Settings.f_defaultJumpForce))
		{
			player.jumpForce = Settings.f_defaultJumpForce;
		}
		player.enabled = true;
		player.isFreeCamera = false;
		CharacterController controller = player.GetComponent<CharacterController>();
		if (controller != null)
		{
			controller.enabled = true;
		}
		KBInput leftover = player.GetComponent<KBInput>();
		if (leftover != null)
		{
			UnityEngine.Object.Destroy(leftover);
		}
		if (player.activeAudioListener != null && !player.activeAudioListener.enabled)
		{
			EnemyControl.ChangeAudioListener(null, reset: true);
		}
		GameUtil.RenderPlayerModels();
	}

	private static void RestoreInput()
	{
		PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
		if (player == null || StartOfRound.Instance == null)
		{
			// Main menu: the game manages its own cursor there.
			return;
		}
		player.playerActions.Enable();
		bool gameWantsCursor = (player.quickMenuManager != null && player.quickMenuManager.isMenuOpen) || player.inTerminalMenu;
		if (!gameWantsCursor)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	private static void ClearState()
	{
		MenuUtil.ResetForEject();
		Settings.debugMessage = "";
		LethalMenu.items.Clear();
		LethalMenu.landmines.Clear();
		LethalMenu.turrets.Clear();
		LethalMenu.doors.Clear();
		LethalMenu.players.Clear();
		LethalMenu.enemies.Clear();
		LethalMenu.steamValves.Clear();
		LethalMenu.bigDoors.Clear();
		LethalMenu.allTerminalObjects.Clear();
		LethalMenu.doorLocks.Clear();
		LethalMenu.teleporters.Clear();
		LethalMenu.interactTriggers.Clear();
		LethalMenu.spikeRoofTraps.Clear();
		LethalMenu.animatedTriggers.Clear();
		LethalMenu.enemyVents.Clear();
		LethalMenu.vehicles.Clear();
		LethalMenu.fogs.Clear();
		LethalMenu.volumes.Clear();
		LethalMenu.vainShrouds = new System.Collections.Generic.List<GameObject>();
		LethalMenu.selectedPlayer = -1;
		ChamHandler.ChamsSetEnabled(enabled: false);
		LethalMenu.ClearInstance();
	}
}
