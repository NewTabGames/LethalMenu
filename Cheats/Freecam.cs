using System;
using LethalMenu.Components;
using LethalMenu.Manager;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class Freecam : Cheat
{
	public static Camera camera;

	private static Light light;

	public static MouseInput mouse;

	public static KBInput movement;

	public static AudioListener audioListener;

	public static bool isStatic;

	public override void Update()
	{
		try
		{
			Stop();
			if (Hack.FreeCam.IsEnabled())
			{
				isStatic = Hack.EnemyControl.IsEnabled();
				if (LethalMenu.localPlayer != null)
				{
					LethalMenu.localPlayer.enabled = false;
					LethalMenu.localPlayer.isFreeCamera = true;
				}
				CreateIfNull();
				if (light != null)
				{
					light.intensity = Settings.f_nvIntensity;
					light.range = Settings.f_nvRange;
				}
				GameUtil.RenderPlayerModels();
				if (!isStatic && camera != null && movement != null && mouse != null)
				{
					camera.transform.SetPositionAndRotation(movement.transform.position, mouse.transform.rotation);
				}
			}
		}
		catch (Exception ex)
		{
			Settings.debugMessage = ex.Message + "\n" + ex.StackTrace;
		}
	}

	private void CreateIfNull()
	{
		if (camera == null && CameraManager.GetBaseCamera() != null)
		{
			camera = GameObjectUtil.CreateCamera("FreeCam", CameraManager.GetBaseCamera().transform);
			camera.enabled = true;
			mouse = camera.gameObject.AddComponent<MouseInput>();
			movement = camera.gameObject.AddComponent<KBInput>();
			audioListener = camera.gameObject.AddComponent<AudioListener>();
			light = GameObjectUtil.CreateLight();
			light.transform.SetParent(camera.transform, worldPositionStays: false);
			CameraManager.GetBaseCamera().enabled = false;
			CameraManager.ActiveCamera = camera;
			EnemyControl.ChangeAudioListener(audioListener);
		}
	}

	public static void Reset()
	{
		Hack.FreeCam.SetToggle(b: false);
		Stop();
	}

	public static void Stop()
	{
		if (Hack.FreeCam.IsEnabled())
		{
			return;
		}
		if (StartOfRound.Instance != null)
		{
			Camera baseCamera = CameraManager.GetBaseCamera();
			if (baseCamera != null)
			{
				CameraManager.ActiveCamera = ((SpectatePlayer.spectatingPlayer == -1) ? baseCamera : SpectatePlayer.camera);
				CameraManager.ActiveCamera.enabled = true;
			}
			if (LethalMenu.localPlayer != null)
			{
				LethalMenu.localPlayer.enabled = true;
				LethalMenu.localPlayer.isFreeCamera = false;
				GameUtil.RenderPlayerModels();
			}
		}
		if (camera != null)
		{
			camera.enabled = false;
			UnityEngine.Object.Destroy(camera.gameObject);
			camera = null;
		}
		mouse = null;
		movement = null;
		if (audioListener != null)
		{
			UnityEngine.Object.Destroy(audioListener.gameObject);
			audioListener = null;
			EnemyControl.ChangeAudioListener(null, reset: true);
		}
		if (light != null)
		{
			UnityEngine.Object.Destroy(light.gameObject);
			light = null;
		}
	}
}
