using System;
using LethalMenu.Manager;
using UnityEngine;

namespace LethalMenu;

public class Cheat : MonoBehaviour
{
	protected static bool WorldToScreen(Camera camera, Vector3 world, out Vector3 screen)
	{
		screen = camera.WorldToViewportPoint(world);
		screen.x *= Screen.width;
		screen.y *= Screen.height;
		screen.y = (float)Screen.height - screen.y;
		return (double)screen.z > 0.0;
	}

	protected static bool WorldToScreen(Vector3 world, out Vector3 screen)
	{
		screen = CameraManager.ActiveCamera.WorldToViewportPoint(world);
		screen.x *= Screen.width;
		screen.y *= Screen.height;
		screen.y = (float)Screen.height - screen.y;
		return (double)screen.z > 0.0;
	}

	protected float GetDistanceToPlayer(Vector3 position)
	{
		return (CameraManager.ActiveCamera != null) ? ((float)Math.Round(Vector3.Distance(CameraManager.ActiveCamera.transform.position, position))) : 0f;
	}

	public virtual void OnGui()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}
}
