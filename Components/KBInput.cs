using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMenu.Components;

internal class KBInput : MonoBehaviour
{
	private float sprintMultiplier = 1f;

	private void Update()
	{
		if (!Cursor.visible)
		{
			Vector3 vector = default;
			if (Keyboard.current.wKey.isPressed)
			{
				vector += transform.forward;
			}
			if (Keyboard.current.sKey.isPressed)
			{
				vector -= transform.forward;
			}
			if (Keyboard.current.aKey.isPressed)
			{
				vector -= transform.right;
			}
			if (Keyboard.current.dKey.isPressed)
			{
				vector += transform.right;
			}
			if (Keyboard.current.spaceKey.isPressed)
			{
				vector += transform.up;
			}
			if (Keyboard.current.ctrlKey.isPressed)
			{
				vector -= transform.up;
			}
			if (!vector.Equals(Vector3.zero))
			{
				sprintMultiplier = (Keyboard.current.shiftKey.isPressed ? Mathf.Min(sprintMultiplier + 5f * Time.deltaTime, 5f) : 1f);
				transform.position += vector * Time.deltaTime * Settings.f_inputMovementSpeed * sprintMultiplier;
			}
		}
	}
}
