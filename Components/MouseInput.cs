using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMenu.Components;

internal class MouseInput : MonoBehaviour
{
	private float Yaw = 0f;

	private float Pitch = 0f;

	internal float CurrentYaw => Yaw;

	internal float CurrentPitch => Pitch;

	/// <summary>Points the view at a world position (used by lunge aim-assist).</summary>
	internal void LookAt(Vector3 from, Vector3 to)
	{
		Vector3 direction = to - from;
		direction.y = 0f;
		if (direction.sqrMagnitude > 0.0001f)
		{
			Yaw = Quaternion.LookRotation(direction).eulerAngles.y;
			Apply();
		}
	}

	internal void SetYaw(float yaw)
	{
		Yaw = (yaw % 360f + 360f) % 360f;
		Apply();
	}

	private void Update()
	{
		if (!Cursor.visible)
		{
			Yaw += Mouse.current.delta.x.ReadValue() * Settings.f_mouseSensitivity;
			Yaw = (Yaw + 360f) % 360f;
			Pitch -= Mouse.current.delta.y.ReadValue() * Settings.f_mouseSensitivity;
			Pitch = Mathf.Clamp(Pitch, -90f, 90f);
			Apply();
		}
	}

	private void Apply()
	{
		transform.eulerAngles = new Vector3(Pitch, Yaw, 0f);
	}
}
