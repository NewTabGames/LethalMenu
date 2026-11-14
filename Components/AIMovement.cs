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

	/// <summary>Set for monsters that climb ladders like a player (the Masked).</summary>
	internal bool CanClimbLadders;

	/// <summary>The ladder being climbed, or null.</summary>
	private InteractTrigger ladder;

	private static InteractTrigger[] ladders = new InteractTrigger[0];

	private static float nextLadderScan;

	private const float LadderClimbSpeed = 4f;

	/// <summary>How close (sideways) you have to walk up to a ladder to grab it.</summary>
	private const float LadderReach = 1.1f;

	internal bool OnLadder => ladder != null;

	/// <summary>Grabbed the ladder from the top while holding W: W does nothing until it's let go once.</summary>
	private bool ladderWaitForRelease;

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
		ladder = null;
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

		if (CanClimbLadders && !Flying && UpdateLadder(input, inputAllowed, deltaTime))
		{
			return;
		}

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

	/// <summary>
	/// Ladders, like a player: walk into one (W, facing it) to grab it, then W climbs up and S climbs down. At the
	/// top you step off onto the floor above, at the bottom you let go; Space jumps off. To go down from the top,
	/// walk into the ladder there and hold S. Returns true while on a ladder (normal walking is skipped).
	/// </summary>
	private bool UpdateLadder(Vector2 input, bool inputAllowed, float deltaTime)
	{
		if (ladder == null)
		{
			if (input.y <= 0.1f)
			{
				return false;
			}
			InteractTrigger found = FindLadder();
			if (found == null)
			{
				return false;
			}
			ladder = found;
			horizontalVelocity = Vector3.zero;
			verticalVelocity = 0f;
			dashTimeRemaining = 0f;
			Vector3 start = transform.position;
			// Grabbed from the top: start just below the top so S takes you down (and W, still held from
			// walking into it, doesn't put you straight back on top).
			ladderWaitForRelease = start.y > ladder.topOfLadderPosition.position.y - 0.5f;
			start.y = Mathf.Clamp(start.y, ladder.bottomOfLadderPosition.position.y, ladder.topOfLadderPosition.position.y - 0.4f);
			Place(start);
		}
		if (ladder == null || !ladder.isActiveAndEnabled)
		{
			ladder = null;
			return false;
		}
		if (ladderWaitForRelease)
		{
			if (input.y <= 0.1f)
			{
				ladderWaitForRelease = false;
			}
			else
			{
				input.y = 0f;
			}
		}
		IsMoving = Mathf.Abs(input.y) > 0.1f;
		IsSprinting = false;
		Transform top = ladder.topOfLadderPosition;
		Transform bottom = ladder.bottomOfLadderPosition;
		if (inputAllowed && Keyboard.spaceKey.wasPressedThisFrame)
		{
			ladder = null;
			verticalVelocity = 2f;
			horizontalVelocity = -(Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward) * 3f;
			return false;
		}
		Vector3 position = transform.position;
		Vector3 rail = ladder.ladderHorizontalPosition != null ? ladder.ladderHorizontalPosition.position : bottom.position;
		position.x = Mathf.MoveTowards(position.x, rail.x, 4f * deltaTime);
		position.z = Mathf.MoveTowards(position.z, rail.z, 4f * deltaTime);
		position.y += input.y * LadderClimbSpeed * deltaTime;
		if (position.y >= top.position.y - 0.3f && input.y > 0f)
		{
			ladder = null;
			Place(top.position);
			return true;
		}
		if (position.y <= bottom.position.y && input.y < 0f)
		{
			ladder = null;
			Place(bottom.position);
			return true;
		}
		Place(position);
		return true;
	}

	private void Place(Vector3 position)
	{
		CharacterController.enabled = false;
		transform.position = position;
		CharacterController.enabled = true;
	}

	/// <summary>The ladder you're walking into: close sideways, within its height, and roughly in front of you.</summary>
	private InteractTrigger FindLadder()
	{
		if (Time.realtimeSinceStartup >= nextLadderScan)
		{
			nextLadderScan = Time.realtimeSinceStartup + 5f;
			ladders = FindObjectsOfType<InteractTrigger>().Where((InteractTrigger t) => t.isLadder && t.topOfLadderPosition != null && t.bottomOfLadderPosition != null).ToArray();
		}
		Vector3 here = transform.position;
		Vector3 forward = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward;
		foreach (InteractTrigger candidate in ladders)
		{
			if (candidate == null || !candidate.isActiveAndEnabled)
			{
				continue;
			}
			float bottomY = candidate.bottomOfLadderPosition.position.y;
			float topY = candidate.topOfLadderPosition.position.y;
			if (here.y < bottomY - 1f || here.y > topY + 1f)
			{
				continue;
			}
			Vector3 rail = candidate.ladderHorizontalPosition != null ? candidate.ladderHorizontalPosition.position : candidate.bottomOfLadderPosition.position;
			Vector3 toRail = rail - here;
			toRail.y = 0f;
			// At the top the rail is at the edge; let you grab it from the top landing too.
			bool atTop = Mathf.Abs(here.y - topY) < 1f;
			float reach = atTop ? LadderReach + 0.8f : LadderReach;
			if (toRail.magnitude > reach)
			{
				continue;
			}
			if (toRail.sqrMagnitude > 0.04f && Vector3.Dot(forward, toRail.normalized) < 0.3f)
			{
				continue;
			}
			return candidate;
		}
		return null;
	}
}
