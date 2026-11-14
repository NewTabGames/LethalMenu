using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;

namespace LethalMenu.Util;

internal class GameObjectUtil
{
	public static Camera CreateCamera(string name, Transform pos, bool copyPlayerTexture = true)
	{
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		Camera camera = new GameObject(name).AddComponent<Camera>();
		camera.transform.position = pos.position;
		camera.transform.rotation = pos.rotation;
		camera.targetTexture = (copyPlayerTexture ? localPlayerController.gameplayCamera.targetTexture : new RenderTexture(1920, 1080, 24));
		camera.cullingMask = localPlayerController.gameplayCamera.cullingMask;
		camera.farClipPlane = localPlayerController.gameplayCamera.farClipPlane;
		camera.nearClipPlane = localPlayerController.gameplayCamera.nearClipPlane;
		return camera;
	}

	public static RawImage CreateMiniCamDisplay(Texture targetTexture)
	{
		RawImage rawImage = new GameObject("SpectateMiniCamDisplay").AddComponent<RawImage>();
		rawImage.rectTransform.anchorMin = new Vector2(1f, 1f);
		rawImage.rectTransform.anchorMax = new Vector2(1f, 1f);
		rawImage.rectTransform.pivot = new Vector2(1f, 1f);
		rawImage.rectTransform.sizeDelta = new Vector2(192f, 108f);
		rawImage.rectTransform.anchoredPosition = new Vector2(1f, 1f);
		rawImage.texture = targetTexture;
		rawImage.transform.SetParent(HUDManager.Instance.playerScreenTexture.transform, worldPositionStays: false);
		return rawImage;
	}

	public static Light CreateLight()
	{
		Light light = Object.Instantiate(LethalMenu.localPlayer.nightVision);
		light.enabled = true;
		light.intensity = Settings.f_nvIntensity;
		light.range = Settings.f_nvRange;
		return light;
	}
}
