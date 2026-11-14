using System.Linq;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMenu.Components;

/// <summary>
/// Character-style movement for a controlled enemy: yaw-relative WASD, smoothed acceleration,
/// sprint/sneak, grounded jumping with coyote time, and scripted dashes (used by the lunge attack).
/// </summary>
internal class AIMovement : MonoBehaviour
{
	private const float JumpForce = 9.2f;

	private const float Gravity = 18f;

	private const float GroundStickForce = -2f;

	private const float CoyoteTime = 0.15f;

	private const float JumpBufferTime = 0.15f;

	private const float Acceleration = 45f;

	private const float Deceleration = 60f;

	private const float SneakMultiplier = 0.5f;

	internal const float MaxSprintSpeed = 16f;

	/// <summary>Walking speed in m/s. Set every frame by EnemyControl.</summary>
	internal float CharacterSpeed = 5f;

	/// <summary>Sprint multiplier applied to CharacterSpeed (final speed is capped at MaxSprintSpeed).</summary>
	internal float CharacterSprintSpeed = 2.8f;

	private Vector3 horizontalVelocity = Vector3.zero;

	private float verticalVelocity;

	private float timeSinceGrounded;

	private float timeSinceJumpPressed = float.MaxValue;

	private Vector3 dashVelocity;

	private float dashTimeRemaining;

	private Keyboard Keyboard = Keyboard.current;

	private KBInput NoClipInput = null;

	private CharacterController CharacterController;

	internal bool IsMoving { get; private set; } = false;

	internal bool IsSprinting { get; private set; } = false;

	internal bool IsDashing => dashTimeRemaining > 0f;

	internal bool IsGrounded => CharacterController != null && CharacterController.isGrounded;

	/// <summary>Flying (manticoils): no gravity, Space rises and Ctrl sinks instead of jump/sneak.</summary>
	internal bool Flying;

	internal float VerticalVelocity => verticalVelocity;

	private const float FlyVerticalSpeed = 6f;

	private const float FlyVerticalAcceleration = 30f;

	internal void SetNoClipMode(bool enabled)
	{
		if ((object)NoClipInput != null)
		{
			NoClipInput.enabled = enabled;
		}
	}

	internal void Init()
	{
		PlayerControllerB localPlayer = LethalMenu.localPlayer;
		if ((object)localPlayer != null)
		{
			gameObject.layer = localPlayer.gameObject.layer;
		}
	}

	internal void SetPosition(Vector3 newPosition)
	{
		if ((object)CharacterController != null)
		{
			CharacterController.enabled = false;
			transform.position = newPosition;
			CharacterController.enabled = true;
		}
		horizontalVelocity = Vector3.zero;
		verticalVelocity = 0f;
		dashTimeRemaining = 0f;
	}

	/// <summary>Moves horizontally along <paramref name="direction"/> at <paramref name="speed"/> for <paramref name="duration"/> seconds, ignoring input.</summary>
	internal void Dash(Vector3 direction, float speed, float duration)
	{
		direction.y = 0f;
		if (direction.sqrMagnitude < 0.0001f)
		{
			return;
		}
		dashVelocity = direction.normalized * speed;
		dashTimeRemaining = duration;
	}

	internal void CalibrateCollision(EnemyAI enemy)
	{
		if ((object)CharacterController != null)
		{
			CharacterController.height = 1f;
			CharacterController.radius = 0.5f;
			CharacterController.center = new Vector3(0f, 0.5f, 0f);
			CharacterController.stepOffset = Mathf.Min(CharacterController.stepOffset, 0.25f);
			CharacterController.slopeLimit = 50f;
			(from collider in enemy.GetComponentsInChildren<Collider>()
				where collider != CharacterController
				select collider).ToList().ForEach((Collider collider) =>
			{
				Physics.IgnoreCollision(CharacterController, collider);
			});
		}
	}

	private void Awake()
	{
		Keyboard = Keyboard.current;
		NoClipInput = gameObject.AddComponent<KBInput>();
		CharacterController = gameObject.AddComponent<CharacterController>();
	}

	private void Update()
	{
		KBInput noClipInput = NoClipInput;
		if ((object)noClipInput != null && noClipInput.enabled)
		{
			return;
		}
		CharacterController characterController = CharacterController;
		if ((object)characterController == null || !characterController.enabled)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		bool inputAllowed = !Cursor.visible;

		Vector2 input = inputAllowed ? new Vector2(Keyboard.dKey.ReadValue() - Keyboard.aKey.ReadValue(), Keyboard.wKey.ReadValue() - Keyboard.sKey.ReadValue()) : Vector2.zero;
		input = Vector2.ClampMagnitude(input, 1f);
		IsMoving = input.sqrMagnitude > 0.01f;
		IsSprinting = IsMoving && inputAllowed && Keyboard.leftShiftKey.isPressed;
		bool sneaking = inputAllowed && Keyboard.leftCtrlKey.isPressed && !IsSprinting && !Flying;

		// Only yaw matters for movement, so looking up/down never slows you down.
		Quaternion yaw = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
		Vector3 wishDirection = yaw * new Vector3(input.x, 0f, input.y);
		float targetSpeed = CharacterSpeed;
		if (IsSprinting)
		{
			targetSpeed = Mathf.Min(CharacterSpeed * CharacterSprintSpeed, MaxSprintSpeed);
		}
		else if (sneaking)
		{
			targetSpeed *= SneakMultiplier;
		}
		Vector3 targetVelocity = wishDirection * targetSpeed;

		if (dashTimeRemaining > 0f)
		{
			dashTimeRemaining -= deltaTime;
			horizontalVelocity = dashVelocity;
			IsSprinting = true;
			IsMoving = true;
		}
		else
		{
			float rate = targetVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude ? Acceleration : Deceleration;
			if (!characterController.isGrounded && !Flying)
			{
				rate *= 0.4f; // reduced air control
			}
			horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * deltaTime);
		}

		if (Flying)
		{
			// Hover: Space up, Ctrl down, Shift makes both faster.
			float climb = inputAllowed ? (Keyboard.spaceKey.ReadValue() - Keyboard.leftCtrlKey.ReadValue()) : 0f;
			float targetVertical = climb * FlyVerticalSpeed * (IsSprinting || (inputAllowed && Keyboard.leftShiftKey.isPressed) ? 1.8f : 1f);
			verticalVelocity = Mathf.MoveTowards(verticalVelocity, targetVertical, FlyVerticalAcceleration * deltaTime);
			timeSinceGrounded = CoyoteTime + 1f;
			timeSinceJumpPressed = float.MaxValue;
		}
		// Gravity + jumping (grounded only, with coyote time and a small input buffer).
		else if (characterController.isGrounded)
		{
			timeSinceGrounded = 0f;
			if (verticalVelocity < 0f)
			{
				verticalVelocity = GroundStickForce;
			}
		}
		else
		{
			timeSinceGrounded += deltaTime;
			verticalVelocity -= Gravity * deltaTime;
		}
		timeSinceJumpPressed = Flying ? float.MaxValue : (inputAllowed && Keyboard.spaceKey.wasPressedThisFrame ? 0f : timeSinceJumpPressed + deltaTime);
		if (!Flying && timeSinceJumpPressed <= JumpBufferTime && timeSinceGrounded <= CoyoteTime)
		{
			verticalVelocity = JumpForce;
			timeSinceGrounded = CoyoteTime + 1f;
			timeSinceJumpPressed = float.MaxValue;
		}

		CollisionFlags flags = characterController.Move((horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime);
		if (Flying && (flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
		{
			verticalVelocity = 0f; // resting on the floor while still in flight mode
		}
		if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
		{
			verticalVelocity = 0f;
		}
		if ((flags & CollisionFlags.Sides) != 0 && dashTimeRemaining > 0f)
		{
			dashTimeRemaining = 0f; // stop the dash when we hit something (a wall or the target)
		}
	}
}
