using System;
using GameNetcodeStuff;
using LethalMenu.Manager;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.UI;

namespace LethalMenu.Cheats;

internal class SpectatePlayer : Cheat
{
	public static int spectatingPlayer = -1;

	public static Camera camera = null;

	public static AudioListener audioListener = null;

	public static int camPlayer = -1;

	private static Camera minicam = null;

	private static RawImage minicamDisplay = null;

	private static Vector2 defaultTooltipPos = Vector2.zero;

	private static RectTransform tooltips = null;

	public override void Update()
	{
		try
		{
			StopSpectating();
			Spectate();
			DisplayMiniCam();
		}
		catch (Exception ex)
		{
			Settings.debugMessage = ex.Message + "\n" + ex.StackTrace;
		}
	}

	public override void OnGui()
	{
		if (Hack.SpectatePlayer.IsEnabled())
		{
			PlayerControllerB spectatedPlayer = GetSpectatedPlayer();
			if (spectatedPlayer == null)
			{
				return;
			}
			VisualUtil.DrawString(new Vector2(Screen.width / 2, 160f), "(Spectating: " + spectatedPlayer.playerUsername + ")", Settings.c_playerESP, centered: true, alignMiddle: true, bold: false, forceOnScreen: true, 32);
		}
		if (Hack.MiniCam.IsEnabled())
		{
			PlayerControllerB playerControllerB = GetCamPlayer();
			if (!(playerControllerB == null))
			{
				float num = minicamDisplay.rectTransform.localPosition.x + minicamDisplay.rectTransform.sizeDelta.x / 2f;
				VisualUtil.DrawString(new Vector2((float)Screen.width - num / 2f, 15f), playerControllerB.playerUsername, Settings.c_playerESP, centered: true, alignMiddle: true, bold: false, forceOnScreen: true, 16);
			}
		}
	}

	public static void Reset()
	{
		CameraManager.ActiveCamera = null;
		spectatingPlayer = -1;
		camPlayer = -1;
		if (camera != null)
		{
			UnityEngine.Object.Destroy(camera.gameObject);
		}
		if (minicam != null)
		{
			UnityEngine.Object.Destroy(minicam.gameObject);
		}
		if (minicamDisplay != null)
		{
			UnityEngine.Object.Destroy(minicamDisplay.gameObject);
		}
		camera = null;
		minicam = null;
		minicamDisplay = null;
		audioListener = null;
		Hack.SpectatePlayer.SetToggle(b: false);
		Hack.MiniCam.SetToggle(b: false);
	}

	public static void StopSpectating()
	{
		if (!StartOfRound.Instance || (!Hack.SpectatePlayer.IsEnabled() && camera != null))
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			PlayerControllerB playerControllerB = localPlayerController.playersManager.allPlayerScripts[spectatingPlayer];
			camera.enabled = false;
			CameraManager.GetBaseCamera().enabled = true;
			CameraManager.ActiveCamera = ((!Hack.FreeCam.IsEnabled()) ? CameraManager.GetBaseCamera() : Freecam.camera);
			spectatingPlayer = -1;
			UnityEngine.Object.Destroy(camera.gameObject);
			camera = null;
			GameUtil.RenderPlayerModels();
			audioListener = null;
			if (LethalMenu.localPlayer != null && !LethalMenu.localPlayer.activeAudioListener.enabled)
			{
				EnemyControl.ChangeAudioListener(null, reset: true);
			}
		}
		if (!StartOfRound.Instance || (!Hack.MiniCam.IsEnabled() && minicam != null))
		{
			PlayerControllerB localPlayerController2 = GameNetworkManager.Instance.localPlayerController;
			PlayerControllerB playerControllerB2 = localPlayerController2.playersManager.allPlayerScripts[camPlayer];
			tooltips.anchoredPosition = defaultTooltipPos;
			tooltips = null;
			camPlayer = -1;
			minicamDisplay.transform.SetParent(null, worldPositionStays: false);
			defaultTooltipPos = Vector2.zero;
			UnityEngine.Object.Destroy(minicam.gameObject);
			if (minicamDisplay != null)
			{
				UnityEngine.Object.Destroy(minicamDisplay.gameObject);
			}
			minicam = null;
			minicamDisplay = null;
			audioListener = null;
			GameUtil.RenderPlayerModels();
			if (LethalMenu.localPlayer != null && !LethalMenu.localPlayer.activeAudioListener.enabled)
			{
				EnemyControl.ChangeAudioListener(null, reset: true);
			}
		}
	}

	private void Spectate()
	{
		if (Hack.SpectatePlayer.IsEnabled())
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			PlayerControllerB playerControllerB = localPlayerController.playersManager.allPlayerScripts[spectatingPlayer];
			if (camera == null)
			{
				camera = GameObjectUtil.CreateCamera("SpectateCamera", CameraManager.GetBaseCamera().transform);
				camera.enabled = true;
				CameraManager.GetBaseCamera().enabled = false;
				CameraManager.ActiveCamera = camera;
				audioListener = camera.gameObject.AddComponent<AudioListener>();
				EnemyControl.ChangeAudioListener(audioListener);
			}
			bool flag = Hack.NightVision.IsEnabled() || playerControllerB.isInsideFactory;
			float intensity = (Hack.NightVision.IsEnabled() ? Settings.f_nvIntensity : Settings.f_defaultNightVisionIntensity);
			float range = (Hack.NightVision.IsEnabled() ? Settings.f_nvRange : Settings.f_defaultNightVisionRange);
			playerControllerB.nightVision.enabled = flag;
			playerControllerB.nightVision.intensity = intensity;
			playerControllerB.nightVision.range = range;
			GameUtil.RenderPlayerModels();
			camera.transform.SetLocalPositionAndRotation(playerControllerB.gameplayCamera.transform.localPosition, playerControllerB.gameplayCamera.transform.localRotation);
			camera.transform.SetPositionAndRotation(playerControllerB.gameplayCamera.transform.position, playerControllerB.gameplayCamera.transform.rotation);
		}
	}

	private void DisplayMiniCam()
	{
		if (Hack.MiniCam.IsEnabled())
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			PlayerControllerB playerControllerB = localPlayerController.playersManager.allPlayerScripts[camPlayer];
			if (minicam == null)
			{
				minicam = GameObjectUtil.CreateCamera("SpectateMiniCam", playerControllerB.gameplayCamera.transform, copyPlayerTexture: false);
				minicamDisplay = GameObjectUtil.CreateMiniCamDisplay(minicam.targetTexture);
			}
			minicam.gameObject.transform.SetLocalPositionAndRotation(playerControllerB.gameObject.transform.localPosition, playerControllerB.gameObject.transform.localRotation);
			minicam.transform.SetPositionAndRotation(playerControllerB.gameplayCamera.transform.position, playerControllerB.gameplayCamera.transform.rotation);
			bool flag = Hack.NightVision.IsEnabled() || playerControllerB.isInsideFactory;
			float intensity = (Hack.NightVision.IsEnabled() ? Settings.f_nvIntensity : Settings.f_defaultNightVisionIntensity);
			float range = (Hack.NightVision.IsEnabled() ? Settings.f_nvRange : Settings.f_defaultNightVisionRange);
			playerControllerB.nightVision.enabled = flag;
			playerControllerB.nightVision.intensity = intensity;
			playerControllerB.nightVision.range = range;
			GameUtil.RenderPlayerModels();
			tooltips = HUDManager.Instance.Tooltips.canvasGroup.gameObject.GetComponent<RectTransform>();
			if (defaultTooltipPos == Vector2.zero)
			{
				defaultTooltipPos = tooltips.anchoredPosition;
			}
			tooltips.anchoredPosition -= new Vector2(0f, 200f);
		}
	}

	public static bool isSpectatingPlayer(PlayerControllerB player)
	{
		if (spectatingPlayer == -1)
		{
			return false;
		}
		string playerUsername = StartOfRound.Instance.allPlayerScripts[spectatingPlayer].playerUsername;
		return spectatingPlayer == (int)player.playerClientId && playerUsername == player.playerUsername;
	}

	public static PlayerControllerB GetSpectatedPlayer()
	{
		if (spectatingPlayer == -1)
		{
			return null;
		}
		return StartOfRound.Instance.allPlayerScripts[spectatingPlayer];
	}

	public static bool isCamPlayer(PlayerControllerB player)
	{
		if (camPlayer == -1)
		{
			return false;
		}
		string playerUsername = StartOfRound.Instance.allPlayerScripts[camPlayer].playerUsername;
		return camPlayer == (int)player.playerClientId && playerUsername == player.playerUsername;
	}

	public static PlayerControllerB GetCamPlayer()
	{
		if (camPlayer == -1)
		{
			return null;
		}
		return StartOfRound.Instance.allPlayerScripts[camPlayer];
	}
}
