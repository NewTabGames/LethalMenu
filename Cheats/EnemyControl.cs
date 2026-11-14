using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using LethalMenu.Components;
using LethalMenu.Handler;
using LethalMenu.Handler.EnemyControl;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace LethalMenu.Cheats;

internal class EnemyControl : Cheat
{
	private static EnemyAI enemy = null;

	/// <summary>Player picked with T (or from the website) for controlled enemies to go after; null = nearest player.</summary>
	internal static PlayerControllerB ChosenTarget;

	/// <summary>The chosen target if it is still a valid living player, otherwise null.</summary>
	internal static PlayerControllerB ValidChosenTarget => (ChosenTarget != null && ChosenTarget.isPlayerControlled && !ChosenTarget.isPlayerDead) ? ChosenTarget : null;

	/// <summary>True when you picked your own body as the target (it is never targeted unless you pick it).</summary>
	internal static bool TargetingYourself => ValidChosenTarget != null && ValidChosenTarget == GameNetworkManager.Instance?.localPlayerController;

	/// <summary>T: nearest -> each other living player -> yourself -> nearest ...</summary>
	internal static void CycleTarget()
	{
		PlayerControllerB local = GameNetworkManager.Instance?.localPlayerController;
		System.Collections.Generic.List<PlayerControllerB> options = new System.Collections.Generic.List<PlayerControllerB>();
		foreach (PlayerControllerB player in StartOfRound.Instance?.allPlayerScripts ?? new PlayerControllerB[0])
		{
			if (player != null && player != local && player.isPlayerControlled && !player.isPlayerDead)
			{
				options.Add(player);
			}
		}
		// Your own body comes last, and only while it is alive.
		if (local != null && local.isPlayerControlled && !local.isPlayerDead)
		{
			options.Add(local);
		}
		int index = ValidChosenTarget == null ? -1 : options.IndexOf(ChosenTarget);
		ChosenTarget = (index + 1 < options.Count) ? options[index + 1] : null;
		if (options.Count == 0)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No living players to target");
		}
	}

	/// <summary>Used by the website.</summary>
	internal static void SetAIDriving(bool aiDriving)
	{
		if (Controlling && IsAIControlled != aiDriving)
		{
			ToggleAIControl();
		}
	}

	/// <summary>The enemy currently being controlled, or null.</summary>
	internal static EnemyAI ControlledEnemy => Controlling ? enemy : null;

	/// <summary>True while you (not the AI) are driving this enemy.</summary>
	internal static bool IsDrivenByYou(EnemyAI candidate)
	{
		return Controlling && !IsAIControlled && (object)candidate != null && (object)candidate == enemy;
	}

	/// <summary>Flight for enemies that can fly (manticoils): no gravity, Space up, Ctrl down.</summary>
	internal static bool IsFlying => movement != null && movement.Flying;

	internal static bool IsOnGround => movement == null || movement.IsGrounded;

	internal static float VerticalSpeed => movement != null ? movement.VerticalVelocity : 0f;

	internal static void SetFlying(bool flying)
	{
		if (movement != null)
		{
			movement.Flying = flying;
		}
	}

	private static GameObject ControllerInstance = null;

	private static MouseInput mouse = null;

	private static AIMovement movement = null;

	private static AudioListener audioListener = null;

	public static bool IsAIControlled = false;

	public static bool Controlling = false;

	private static bool NoClipEnabled = false;

	private const float TeleportDoorCooldown = 2.5f;

	private const float DoorInteractionCooldown = 0.7f;

	private float DoorCooldownRemaining = 0f;

	private float TeleportCooldownRemaining = 0f;

	private const float LungeCooldown = 1.5f;

	private const float LungeSpeed = 22f;

	private const float LungeRange = 14f;

	private const float LungeAssistAngle = 45f;

	private const float SkillSpamCooldown = 0.25f;

	private const float MinWalkSpeed = 3.5f;

	private const float MaxWalkSpeed = 9f;

	private static float LungeCooldownRemaining = 0f;

	private static float PrimaryCooldownRemaining = 0f;

	private static float SecondaryCooldownRemaining = 0f;

	private static bool SecondaryHeld = false;

	private static float BaseWalkSpeed = 4.5f;

	private static float nextOwnershipRequest;

	/// <summary>Sprint speed divided by walk speed for the enemy being controlled.</summary>
	private static float BaseSprintMultiplier = 2.8f;

	/// <summary>
	/// Walk and run speeds in m/s, taken from what each monster's own AI uses in the game (its normal roaming
	/// speed and its chasing speed). Running is still capped at AIMovement.MaxSprintSpeed.
	/// </summary>
	private static readonly Dictionary<string, (float Walk, float Run)> GameSpeeds = new Dictionary<string, (float, float)>
	{
		{ "BaboonBirdAI", (9f, 12f) },
		{ "BlobAI", (0.6f, 3f) },
		{ "BushWolfEnemy", (5f, 14f) },
		{ "ButlerBeesEnemyAI", (4f, 10f) },
		{ "ButlerEnemyAI", (4f, 9f) },
		{ "CadaverBloomAI", (5f, 10f) },
		{ "CaveDwellerAI", (4f, 6.25f) },
		{ "CentipedeAI", (3f, 5.2f) },
		{ "ClaySurgeonAI", (3.5f, 6f) },
		{ "CrawlerAI", (7f, 16f) },
		{ "DocileLocustBeesAI", (3f, 6f) },
		{ "DoublewingAI", (3f, 6f) },
		{ "DressGirlAI", (5.25f, 8f) },
		{ "FlowerSnakeEnemy", (4.5f, 7f) },
		{ "FlowermanAI", (4f, 9f) },
		{ "ForestGiantAI", (5f, 8f) },
		{ "GiantKiwiAI", (4f, 16f) },
		{ "HoarderBugAI", (6f, 16f) },
		{ "JesterAI", (5f, 16f) },
		{ "LassoManAI", (5f, 6f) },
		{ "MaskedPlayerEnemy", (3.8f, 8f) },
		{ "MouthDogAI", (3.5f, 16f) },
		{ "NutcrackerEnemyAI", (5.5f, 7f) },
		{ "PufferAI", (4f, 12f) },
		{ "PumaAI", (6f, 14f) },
		{ "RadMechAI", (4f, 8f) },
		{ "RedLocustBees", (4f, 10.3f) },
		{ "SandSpiderAI", (4.5f, 6f) },
		{ "SandWormAI", (4f, 8f) },
		{ "SpringManAI", (6f, 14f) },
		{ "StingrayAI", (5.5f, 11f) },
		{ "TestEnemy", (5f, 12f) }
	};

	/// <summary>A flying manticoil glides at 5-19 m/s in the game.</summary>
	private const float FlyWalkSpeed = 7f;

	private const float FlyRunMultiplier = 2.2f;

	private static float CameraHeight = 2f;

	private static float CameraBaseDistance = 4.5f;

	private static float CameraZoom = 1f;

	private static float CameraCurrentDistance = 4.5f;

	private static Dictionary<Type, IController> EnemyControllers { get; } = new Dictionary<Type, IController>
	{
		{
			typeof(CentipedeAI),
			new CentipedeController()
		},
		{
			typeof(FlowermanAI),
			new FlowermanController()
		},
		{
			typeof(ForestGiantAI),
			new ForestGiantController()
		},
		{
			typeof(HoarderBugAI),
			new HoarderBugController()
		},
		{
			typeof(JesterAI),
			new JesterController()
		},
		{
			typeof(NutcrackerEnemyAI),
			new NutcrackerController()
		},
		{
			typeof(PufferAI),
			new PufferController()
		},
		{
			typeof(BaboonBirdAI),
			new BaboonBirdController()
		},
		{
			typeof(SandWormAI),
			new SandWormController()
		},
		{
			typeof(MouthDogAI),
			new MouthDogController()
		},
		{
			typeof(MaskedPlayerEnemy),
			new MaskedPlayerController()
		},
		{
			typeof(SpringManAI),
			new SpringManController()
		},
		{
			typeof(BlobAI),
			new BlobController()
		},
		{
			typeof(TestEnemy),
			new TestEnemyController()
		},
		{
			typeof(LassoManAI),
			new LassoManController()
		},
		{
			typeof(CrawlerAI),
			new CrawlerController()
		},
		{
			typeof(SandSpiderAI),
			new SandSpiderController()
		},
		{
			typeof(RedLocustBees),
			new RedLocustBeesController()
		},
		{
			typeof(ButlerEnemyAI),
			new ButlerController()
		},
		{
			typeof(ButlerBeesEnemyAI),
			new ButlerBeesController()
		},
		{
			typeof(FlowerSnakeEnemy),
			new FlowerSnakeController()
		},
		{
			typeof(DoublewingAI),
			new DoublewingController()
		},
		{
			typeof(DressGirlAI),
			new DressGirlController()
		},
		{
			typeof(ClaySurgeonAI),
			new ClaySurgeonController()
		},
		{
			typeof(CaveDwellerAI),
			new CaveDwellerController()
		},
		{
			typeof(DocileLocustBeesAI),
			new DocileLocustBeesController()
		},
		{
			typeof(RadMechAI),
			new RadMechController()
		},
		{
			typeof(BushWolfEnemy),
			new BushWolfController()
		},
		{
			typeof(CadaverBloomAI),
			new CadaverBloomController()
		},
		{
			typeof(GiantKiwiAI),
			new GiantKiwiController()
		},
		{
			typeof(PumaAI),
			new PumaController()
		},
		{
			typeof(StingrayAI),
			new StingrayController()
		}
	};

	public static void Control(EnemyAI enemy)
	{
		if (enemy == null || enemy.isEnemyDead)
		{
			return;
		}
		if (ControllerInstance != null && EnemyControl.enemy != null)
		{
			// Already controlling something. Only one monster is ever controlled at a time.
			bool sameMonster = (object)EnemyControl.enemy == enemy;
			bool stillWanted = Hack.EnemyControl.IsEnabled(); // the in-game button has just switched this off
			if (sameMonster && stillWanted)
			{
				return; // asked to control the one we already have (website "Start" pressed twice)
			}
			Controlling = true; // let StopControl run even if this is the very first frame of control
			StopControl(); // puts the current monster back on the ground and hands it to its AI
			if (sameMonster)
			{
				return; // in-game button pressed again on the same monster: that means stop
			}
			Hack.EnemyControl.SetToggle(b: true); // switching to a different monster
		}
		if (ControllerInstance != null)
		{
			UnityEngine.Object.Destroy(ControllerInstance);
			ControllerInstance = null;
			mouse = null;
			movement = null;
			audioListener = null;
		}
		EnemyControl.enemy = enemy;
		IController value = GetControllerForEnemy(enemy);
		if (value is DefaultController)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No custom skills for this enemy, movement only.");
		}
		value.OnTakeControl(enemy);
		if (enemy.IsSpawned)
		{
			enemy.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
			nextOwnershipRequest = Time.time + 0.25f;
		}
		ControllerInstance = new GameObject("EnemyController");
		ControllerInstance.transform.position = enemy.transform.position;
		ControllerInstance.transform.rotation = enemy.transform.rotation;
		mouse = ControllerInstance.AddComponent<MouseInput>();
		movement = ControllerInstance.AddComponent<AIMovement>();
		audioListener = ControllerInstance.AddComponent<AudioListener>();
		movement.Init();
		movement.CalibrateCollision(enemy);
		movement.SetNoClipMode(enabled: false);
		movement.SetPosition(enemy.transform.position);
		movement.CharacterSprintSpeed = SprintMultiplier();
		mouse.SetYaw(enemy.transform.eulerAngles.y);
		CalibrateForEnemy(enemy);
		LungeCooldownRemaining = 0f;
		PrimaryCooldownRemaining = 0f;
		SecondaryCooldownRemaining = 0f;
		SecondaryHeld = false;
		ChosenTarget = null;
		SetAIControl(enableAI: false);
		ChangeAudioListener(audioListener);
	}

	public static void StopControl()
	{
		if (!Controlling)
		{
			return;
		}
		Controlling = false;
		Hack.FreeCam.SetToggle(b: false);
		Hack.EnemyControl.SetToggle(b: false);
		if (Settings.isMenuOpen)
		{
			PlayerControllerB localPlayer = LethalMenu.localPlayer;
			if ((((object)localPlayer != null) ? new bool?(!localPlayer.playerActions.Movement.enabled) : ((bool?)null)).Value)
			{
				LethalMenu.localPlayer?.playerActions.Enable();
			}
		}
		if (enemy?.agent != null)
		{
			// Hand the AI back a monster that is standing on its walkable floor (nav mesh), even if it was
			// left on a ceiling, a ledge or inside a wall; otherwise its AI is stuck and logs errors every tick.
			UpdateEnemyPosition();
			SnapToNavMesh(enemy);
			enemy.agent.updatePosition = true;
			enemy.agent.updateRotation = true;
			if (enemy.agent.isOnNavMesh)
			{
				enemy.agent.isStopped = false;
			}
		}
		IController value = GetControllerForEnemy(enemy);
		if (value != null)
		{
			try
			{
				value.OnReleaseControl(enemy);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Release control: " + ex.Message);
			}
		}
		// Everything below must run even if the monster despawned while controlled (no controller is found
		// for a monster that no longer exists); otherwise the camera rig and its audio listener were left behind.
		ChangeAudioListener(null, reset: true);
		if (LethalMenu.localPlayer != null)
		{
			if (LethalMenu.localPlayer.isPlayerDead && HUDManager.Instance != null)
			{
				HUDManager.Instance.holdButtonToEndGameEarlyMeter.gameObject.SetActive(value: true);
			}
			if (LethalMenu.localPlayer.cursorTip != null)
			{
				LethalMenu.localPlayer.cursorTip.text = "";
			}
		}
		IsAIControlled = false;
		if (ControllerInstance != null)
		{
			UnityEngine.Object.Destroy(ControllerInstance);
		}
		enemy = null;
		ControllerInstance = null;
		mouse = null;
		movement = null;
		audioListener = null;
	}

	public override void Update()
	{
		if (!Hack.EnemyControl.IsEnabled() || enemy == null)
		{
			StopControl();
			return;
		}
		if (!Hack.FreeCam.IsEnabled())
		{
			Hack.FreeCam.Execute();
		}
		if (Freecam.camera == null || HUDManager.Instance == null)
		{
			return;
		}
		IController value = GetControllerForEnemy(enemy);
		if (enemy.isEnemyDead)
		{
			value.OnDeath(enemy);
			StopControl();
			return;
		}
		if (LethalMenu.localPlayer.isPlayerDead)
		{
			HUDManager.Instance.Reflect().SetValue("holdButtonToEndGameEarlyHoldTime", 0f);
		}
		if (LethalMenu.localPlayer.isPlayerDead)
		{
			HUDManager.Instance.holdButtonToEndGameEarlyMeter.gameObject.SetActive(value: false);
		}
		if (enemy.agent == null || !enemy.agent)
		{
			return;
		}
		UpdateCooldowns();
		// Ask the host to make us the owner only while we aren't, and at most 4 times a second: the answer takes
		// a network round trip, and every extra request is another ownership change sent to every player.
		if (enemy.IsSpawned && !enemy.IsOwner && Time.time >= nextOwnershipRequest)
		{
			nextOwnershipRequest = Time.time + 0.25f;
			enemy.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
		}
		Controlling = true;
		try
		{
			value.Update(enemy, IsAIControlled);
			InteractWithAmbient(enemy, value);
			HandleInput(value);
			if (enemy == null)
			{
				return; // Delete / F11 released or killed it this frame
			}
			bool ableToMove = value.IsAbleToMove(enemy);
			if (movement != null)
			{
				// Frozen enemies (mid-animation) must not keep sliding or dashing.
				movement.enabled = !IsAIControlled && ableToMove;
			}
			if (!IsAIControlled && ableToMove)
			{
				movement.CharacterSpeed = GetWalkSpeed(value);
				if (value.IsAbleToRotate(enemy))
				{
					UpdateEnemyRotation();
				}
				UpdateEnemyPosition();
				value.OnMovement(enemy, movement.IsMoving, movement.IsSprinting);
			}
		}
		catch (Exception ex)
		{
			// Before, an error here skipped the camera update every frame, freezing the view.
			Settings.debugMessage = "Enemy control: " + ex.Message + "\n" + ex.StackTrace;
		}
		try
		{
			HintLines = BuildSkillHint(enemy, value);
		}
		catch (Exception ex)
		{
			HintLines = new string[2] { "Controlling: " + enemy.ShownName() + "  (F11: stop)", "Skill info unavailable: " + ex.Message };
		}
		MoveCamera();
	}

	/// <summary>
	/// The AI changes agent.speed per state and often sets it to 0 (hiding, cloaked, winding up...).
	/// Follow it when it's sensible, but never let it freeze or rocket the controlled enemy.
	/// </summary>
	private static float GetWalkSpeed(IController controller)
	{
		if (IsFlying)
		{
			movement.CharacterSprintSpeed = FlyRunMultiplier;
			return FlyWalkSpeed;
		}
		movement.CharacterSprintSpeed = BaseSprintMultiplier;
		// Known monsters always walk at their own game speed (the blob crawls, a thumper is quick).
		if (GameSpeeds.ContainsKey(enemy.GetType().Name) || !controller.SyncAnimationSpeedEnabled(enemy) || enemy.agent == null)
		{
			return BaseWalkSpeed;
		}
		return Mathf.Clamp(Mathf.Max(enemy.agent.speed, BaseWalkSpeed), MinWalkSpeed, MaxWalkSpeed);
	}

	/// <summary>Monsters whose model can't be measured well (swarms, mostly-underground, effect-heavy): body height in meters.</summary>
	private static readonly Dictionary<string, float> CameraHeightOverrides = new Dictionary<string, float>
	{
		{ "RedLocustBees", 1.2f },
		{ "ButlerBeesEnemyAI", 1.2f },
		{ "DocileLocustBeesAI", 1f },
		{ "SandWormAI", 5f },
		{ "DressGirlAI", 1.7f },
		{ "BlobAI", 1.2f },
		{ "CentipedeAI", 0.9f }
	};

	/// <summary>
	/// Monsters whose speeds come from values in the game's data files, read live from the monster (formulas from
	/// the game's AI code): Butler walks at idleMovementSpeedBase and chases at +5; the Kidnapper Fox moves at
	/// 5x / 10x its speedMultiplier; an adult Maneater roams at 6 and chases at chaseSpeed (a baby at 4 / 6.25).
	/// </summary>
	private static void ExactSpeeds(EnemyAI enemy, ref (float Walk, float Run) speeds)
	{
		try
		{
			HarmonyLib.Traverse t = HarmonyLib.Traverse.Create(enemy);
			if (enemy is ButlerEnemyAI)
			{
				float b = t.Field("idleMovementSpeedBase").GetValue<float>();
				if (b > 0.1f) speeds = (b, b + 5f);
			}
			else if (enemy is BushWolfEnemy)
			{
				float m = t.Field("speedMultiplier").GetValue<float>();
				if (m > 0.05f) speeds = (5f * m, 10f * m);
			}
			else if (enemy is CaveDwellerAI)
			{
				GameObject baby = t.Field("babyContainer").GetValue<GameObject>();
				float chase = t.Field("chaseSpeed").GetValue<float>();
				speeds = (baby != null && baby.activeSelf) ? (4f, 6.25f) : (6f, chase > 1f ? chase : 10f);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Exact speed read failed: " + ex.Message);
		}
	}

	private static void CalibrateForEnemy(EnemyAI enemy)
	{
		if (GameSpeeds.TryGetValue(enemy.GetType().Name, out var speeds))
		{
			ExactSpeeds(enemy, ref speeds);
			BaseWalkSpeed = speeds.Walk;
			BaseSprintMultiplier = speeds.Run / speeds.Walk;
		}
		else
		{
			// Monsters added by game updates or other mods: go by what their AI is doing right now.
			float agentSpeed = enemy.agent != null ? enemy.agent.speed : 0f;
			BaseWalkSpeed = Mathf.Clamp(agentSpeed > 0.5f ? agentSpeed : 4.5f, MinWalkSpeed, 7f);
			BaseSprintMultiplier = SprintMultiplier();
		}
		CameraZoom = 1f;
		float height = MeasureBodyHeight(enemy);
		// Framing: pivot a bit below the top of the body, camera a little more than one body-height behind.
		CameraHeight = height * 0.7f + 0.5f;
		CameraBaseDistance = Mathf.Clamp(height * 1f + 2.2f, 2.8f, 13f);
		CameraCurrentDistance = CameraBaseDistance;
	}

	/// <summary>
	/// Height of the enemy's body above its feet, from its actual models only. Particle/VFX/line effects report
	/// huge boxes (a bee swarm measured ~12m tall), and some enemies carry parts far from the body.
	/// </summary>
	private static float MeasureBodyHeight(EnemyAI enemy)
	{
		for (Type type = enemy.GetType(); type != null && type != typeof(EnemyAI); type = type.BaseType)
		{
			if (CameraHeightOverrides.TryGetValue(type.Name, out var fixedHeight))
			{
				return fixedHeight;
			}
		}
		Vector3 feet = enemy.transform.position;
		float top = float.MinValue;
		foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
		{
			if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
			{
				continue;
			}
			Bounds b = renderer.bounds;
			Vector2 offset = new Vector2(b.center.x - feet.x, b.center.z - feet.z);
			if (offset.magnitude > 8f || b.size.y > 25f)
			{
				continue; // detached prop or a giant helper mesh, not the body
			}
			top = Mathf.Max(top, b.max.y);
		}
		float height = (top == float.MinValue) ? 1.8f : top - feet.y;
		return Mathf.Clamp(height, 0.8f, 10f);
	}

	public static void ChangeAudioListener(AudioListener audioListener = null, bool reset = false)
	{
		if (reset && StartOfRound.Instance != null && LethalMenu.localPlayer != null)
		{
			LethalMenu.localPlayer.activeAudioListener.enabled = true;
			StartOfRound.Instance.audioListener = LethalMenu.localPlayer.activeAudioListener;
		}
		else if (audioListener != null)
		{
			LethalMenu.localPlayer.activeAudioListener.enabled = false;
			StartOfRound.Instance.audioListener = audioListener;
		}
	}

	private static readonly IController FallbackController = new DefaultController();

	private static IController GetControllerForEnemy(EnemyAI e)
	{
		if (e == null)
		{
			return null;
		}
		// Walk up the type hierarchy so subclasses (e.g. modded variants) reuse their base enemy's controller.
		for (Type type = e.GetType(); type != null && type != typeof(EnemyAI); type = type.BaseType)
		{
			if (EnemyControllers.TryGetValue(type, out var value) && value != null)
			{
				return value;
			}
		}
		return FallbackController;
	}

	/// <summary>Lines of the small controls box drawn in the bottom-left corner while controlling an enemy.</summary>
	private static string[] HintLines = new string[0];

	private static GUIStyle hintStyle;

	private static string[] BuildSkillHint(EnemyAI enemy, IController controller)
	{
		System.Collections.Generic.List<string> lines = new System.Collections.Generic.List<string>();
		string enemyName = enemy.ShownName();
		lines.Add("Controlling: " + enemyName + "   (F11: stop controlling)");
		PlayerControllerB chosen = ValidChosenTarget;
		string targetLine = "Target: " + (chosen == null ? "nearest other player" : (TargetingYourself ? "YOURSELF (" + chosen.playerUsername + ")" : chosen.playerUsername)) + "   (T: change)";
		if (IsAIControlled)
		{
			lines.Add("The AI is driving. Your clicks do nothing.");
			lines.Add(targetLine);
			lines.Add("F9: take back control  |  F11: stop controlling  |  Delete: kill");
			return lines.ToArray();
		}
		string primary = controller.GetPrimarySkillName(enemy);
		string secondary = controller.GetSecondarySkillName(enemy);
		if (!string.IsNullOrEmpty(primary) && !(controller is DefaultController))
		{
			lines.Add(SkillLine("Left click", primary));
		}
		if (!string.IsNullOrEmpty(secondary))
		{
			lines.Add(SkillLine("Right click", secondary));
		}
		lines.Add(targetLine);
		lines.Add("F / Middle click: Lunge " + (LungeCooldownRemaining > 0f ? $"({LungeCooldownRemaining:0.0}s)" : "(ready)"));
		lines.Add(IsFlying ? "Space: Fly up  |  Ctrl: Fly down  |  Shift: Faster  |  Scroll: Zoom" : "Shift: Sprint  |  Ctrl: Sneak  |  Space: Jump  |  Scroll: Zoom");
		lines.Add("F9: let AI drive  |  F10: NoClip" + (NoClipEnabled ? " (on)" : "") + "  |  F11: stop controlling  |  Delete: kill");
		return lines.ToArray();
	}

	/// <summary>"(HOLD) Chase" becomes "Hold right click: Chase".</summary>
	private static string SkillLine(string button, string skill)
	{
		const string holdTag = "(HOLD)";
		if (skill.StartsWith(holdTag))
		{
			return "Hold " + button.ToLowerInvariant() + ": " + skill.Substring(holdTag.Length).Trim();
		}
		return button + ": " + skill;
	}

	public override void OnGui()
	{
		if (!Controlling || enemy == null || HintLines.Length == 0 || Settings.isMenuOpen || Settings.b_StealthMode)
		{
			return;
		}
		if (hintStyle == null)
		{
			hintStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 13,
				wordWrap = false,
				richText = false
			};
			hintStyle.normal.textColor = Color.white;
		}
		const float padding = 8f;
		const float lineGap = 2f;
		float width = 0f;
		float lineHeight = hintStyle.CalcSize(new GUIContent("Ag")).y;
		foreach (string line in HintLines)
		{
			width = Mathf.Max(width, hintStyle.CalcSize(new GUIContent(line)).x);
		}
		float height = HintLines.Length * (lineHeight + lineGap) - lineGap;
		Rect box = new Rect(12f, Screen.height - height - padding * 2f - 12f, width + padding * 2f, height + padding * 2f);
		Color previous = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, 0.6f);
		GUI.DrawTexture(box, Texture2D.whiteTexture);
		GUI.color = previous;
		for (int i = 0; i < HintLines.Length; i++)
		{
			hintStyle.fontStyle = (i == 0) ? FontStyle.Bold : FontStyle.Normal;
			GUI.Label(new Rect(box.x + padding, box.y + padding + i * (lineHeight + lineGap), width + 4f, lineHeight), HintLines[i], hintStyle);
		}
	}

	private void UpdateCooldowns()
	{
		DoorCooldownRemaining = Mathf.Clamp(DoorCooldownRemaining - Time.deltaTime, 0f, 0.7f);
		TeleportCooldownRemaining = Mathf.Clamp(TeleportCooldownRemaining - Time.deltaTime, 0f, 2.5f);
		LungeCooldownRemaining = Mathf.Max(LungeCooldownRemaining - Time.deltaTime, 0f);
		PrimaryCooldownRemaining = Mathf.Max(PrimaryCooldownRemaining - Time.deltaTime, 0f);
		SecondaryCooldownRemaining = Mathf.Max(SecondaryCooldownRemaining - Time.deltaTime, 0f);
	}

	/// <summary>Third-person orbit camera: follows mouse pitch/yaw, scales with enemy size, zooms with the scroll wheel and doesn't clip through walls.</summary>
	private void MoveCamera()
	{
		if (enemy == null || Freecam.camera == null)
		{
			return;
		}
		// Freecam's own look/fly components live on the same camera; keep them from fighting the orbit camera.
		if (Freecam.mouse != null && Freecam.mouse.enabled)
		{
			Freecam.mouse.enabled = false;
		}
		if (Freecam.movement != null && Freecam.movement.enabled)
		{
			Freecam.movement.enabled = false;
		}
		if (!Cursor.visible)
		{
			float scroll = Mouse.current.scroll.y.ReadValue();
			if (Mathf.Abs(scroll) > 0.01f)
			{
				CameraZoom = Mathf.Clamp(CameraZoom - Mathf.Sign(scroll) * 0.12f, 0.35f, 2.5f);
			}
		}
		float yaw = mouse != null ? mouse.CurrentYaw : enemy.transform.eulerAngles.y;
		float pitch = mouse != null ? Mathf.Clamp(mouse.CurrentPitch, -40f, 70f) : 15f;
		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
		Vector3 pivot = enemy.transform.position + Vector3.up * CameraHeight;
		float desiredDistance = CameraBaseDistance * CameraZoom;
		float allowedDistance = desiredDistance;
		int mask = StartOfRound.Instance != null ? StartOfRound.Instance.collidersAndRoomMaskAndDefault : Physics.DefaultRaycastLayers;
		if (Physics.SphereCast(pivot, 0.25f, rotation * Vector3.back, out RaycastHit hit, desiredDistance, mask, QueryTriggerInteraction.Ignore))
		{
			allowedDistance = Mathf.Max(hit.distance - 0.1f, 0.5f);
		}
		// Pull in instantly when blocked, ease back out when clear.
		CameraCurrentDistance = allowedDistance < CameraCurrentDistance ? allowedDistance : Mathf.Lerp(CameraCurrentDistance, allowedDistance, Time.deltaTime * 6f);
		Freecam.camera.transform.SetPositionAndRotation(pivot - rotation * Vector3.forward * CameraCurrentDistance, rotation);
	}

	/// <summary>Universal attack: dash at the nearest player in front (with aim snap) so the enemy's own contact damage lands.</summary>
	private void Lunge(IController controller)
	{
		if (enemy == null || movement == null || mouse == null || LungeCooldownRemaining > 0f || IsAIControlled || NoClipEnabled || !controller.IsAbleToMove(enemy))
		{
			return;
		}
		PlayerControllerB target = FindLungeTarget();
		Vector3 direction;
		float distance;
		if (target != null)
		{
			mouse.LookAt(enemy.transform.position, target.transform.position);
			UpdateEnemyRotation();
			direction = target.transform.position - enemy.transform.position;
			direction.y = 0f;
			distance = direction.magnitude;
		}
		else
		{
			direction = Quaternion.Euler(0f, mouse.CurrentYaw, 0f) * Vector3.forward;
			distance = 6f;
		}
		// A blob lunges like a blob: 1.4x its own run speed, capped at the old fixed speed.
		float lungeSpeed = Mathf.Clamp(Mathf.Min(BaseWalkSpeed * BaseSprintMultiplier, AIMovement.MaxSprintSpeed) * 1.4f, 2f, LungeSpeed);
		float duration = Mathf.Clamp((distance + 1f) / lungeSpeed, 0.15f, 0.65f);
		movement.Dash(direction, lungeSpeed, duration);
		LungeCooldownRemaining = LungeCooldown;
	}

	private static PlayerControllerB FindLungeTarget()
	{
		if (StartOfRound.Instance == null)
		{
			return null;
		}
		Vector3 forward = Quaternion.Euler(0f, mouse.CurrentYaw, 0f) * Vector3.forward;
		Vector3 origin = enemy.transform.position + Vector3.up;
		PlayerControllerB best = null;
		float bestScore = float.MaxValue;
		foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
		{
			if (player == null || !player.isPlayerControlled || player.isPlayerDead || (player == LethalMenu.localPlayer && !TargetingYourself))
			{
				continue;
			}
			Vector3 toPlayer = player.transform.position - enemy.transform.position;
			float verticalGap = Mathf.Abs(toPlayer.y);
			toPlayer.y = 0f;
			float distance = toPlayer.magnitude;
			if (distance > LungeRange || verticalGap > 4f)
			{
				continue;
			}
			float angle = Vector3.Angle(forward, toPlayer);
			if (angle > LungeAssistAngle)
			{
				continue;
			}
			if (Physics.Linecast(origin, player.transform.position + Vector3.up, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
			{
				continue;
			}
			float score = distance + angle * 0.1f; // prefer whoever you're actually aiming at
			if (score < bestScore)
			{
				bestScore = score;
				best = player;
			}
		}
		return best;
	}

	private static void UpdateEnemyPosition()
	{
		if (!(enemy == null) && !(movement == null) && !(mouse == null))
		{
			Vector3 eulerAngles = enemy.transform.eulerAngles;
			eulerAngles.y = mouse.transform.eulerAngles.y;
			enemy.transform.eulerAngles = eulerAngles;
			enemy.transform.position = movement.transform.position;
		}
	}

	private static void UpdateEnemyRotation()
	{
		if (!(movement == null))
		{
			movement.transform.rotation = mouse.transform.rotation;
		}
	}

	private void HandleInput(IController controller)
	{
		// Clicking around the menu must not fire skills.
		bool blocked = Cursor.visible || Settings.isMenuOpen || IsAIControlled;
		if (blocked)
		{
			if (SecondaryHeld)
			{
				SecondaryHeld = false;
				ReleaseSecondarySkill();
			}
		}
		else
		{
			if (Mouse.current.leftButton.wasPressedThisFrame && PrimaryCooldownRemaining <= 0f)
			{
				PrimaryCooldownRemaining = SkillSpamCooldown;
				UsePrimarySkill();
			}
			if (Mouse.current.rightButton.wasPressedThisFrame && SecondaryCooldownRemaining <= 0f)
			{
				SecondaryCooldownRemaining = SkillSpamCooldown;
				SecondaryHeld = true;
				UseSecondarySkill();
			}
			if (Mouse.current.rightButton.isPressed && SecondaryHeld)
			{
				OnSecondarySkillHold();
			}
			if (!Mouse.current.rightButton.isPressed && SecondaryHeld)
			{
				SecondaryHeld = false;
				ReleaseSecondarySkill();
			}
			if (Keyboard.current.fKey.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)
			{
				Lunge(controller);
			}
		}
		if (Keyboard.current.f9Key.wasPressedThisFrame)
		{
			ToggleAIControl();
		}
		if (Keyboard.current.tKey.wasPressedThisFrame && !Cursor.visible && !Settings.isMenuOpen)
		{
			CycleTarget();
		}
		if (Keyboard.current.f10Key.wasPressedThisFrame)
		{
			ToggleNoClip();
		}
		if (Keyboard.current.f11Key.wasPressedThisFrame)
		{
			Hack.EnemyControl.SetToggle(b: false);
		}
		if (Keyboard.current.deleteKey.wasPressedThisFrame)
		{
			EnemyAI target = enemy;
			StopControl();
			target?.Handle().Kill();
		}
	}

	public static float InteractRange()
	{
		return GetControllerForEnemy(enemy).InteractRange(enemy);
	}

	public static float SprintMultiplier()
	{
		return GetControllerForEnemy(enemy).SprintMultiplier(enemy);
	}

	public static void ToggleAIControl()
	{
		if ((object)enemy?.agent != null && (object)movement != null && (object)mouse != null)
		{
			IsAIControlled = !IsAIControlled;
			SetAIControl(IsAIControlled);
		}
	}

	/// <summary>
	/// Puts the enemy's AI agent on the closest walkable spot (nav mesh) within 20m and syncs it.
	/// Returns false if there is no nav mesh nearby (the agent then stays where it is).
	/// </summary>
	private static bool SnapToNavMesh(EnemyAI target)
	{
		NavMeshAgent agent = target != null ? target.agent : null;
		if (agent == null || !agent.enabled)
		{
			return false;
		}
		Vector3 position = target.transform.position;
		if (!NavMesh.SamplePosition(position, out var nearest, agent.isOnNavMesh ? 2f : 20f, agent.areaMask) && !NavMesh.SamplePosition(position, out nearest, 20f, NavMesh.AllAreas))
		{
			// High in the air (a flying manticoil): look for the ground straight below, then walkable floor near it.
			// Without this the monster jumped back to wherever its AI last stood when control began.
			bool found = false;
			if (StartOfRound.Instance != null && Physics.Raycast(position, Vector3.down, out RaycastHit ground, 1000f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
			{
				found = NavMesh.SamplePosition(ground.point, out nearest, 30f, NavMesh.AllAreas);
			}
			for (float radius = 60f; !found && radius <= 480f; radius *= 2f)
			{
				found = NavMesh.SamplePosition(position, out nearest, radius, NavMesh.AllAreas);
			}
			if (!found)
			{
				return false;
			}
		}
		agent.Warp(nearest.position);
		target.transform.position = nearest.position;
		target.SyncPositionToClients();
		return agent.isOnNavMesh;
	}

	private static void SetAIControl(bool enableAI)
	{
		if ((object)movement != null && (object)enemy != null && (object)enemy.agent != null)
		{
			if (enableAI)
			{
				SnapToNavMesh(enemy);
			}
			if (NoClipEnabled)
			{
				NoClipEnabled = false;
				movement.SetNoClipMode(enabled: false);
			}
			if (enableAI)
			{
				movement.Flying = false; // the AI walks it on the ground (it was snapped down above)
			}
			enemy.agent.updatePosition = enableAI;
			enemy.agent.updateRotation = enableAI;
			if (enemy.agent.isOnNavMesh)
			{
				enemy.agent.isStopped = !enableAI;
			}
			movement.SetPosition(enemy.transform.position);
			movement.enabled = !enableAI;
		}
	}

	public void HandleEntranceDoors(EnemyAI enemy, RaycastHit hit)
	{
		if (!(TeleportCooldownRemaining > 0f) && hit.collider.gameObject.TryGetComponent<EntranceTeleport>(out var component))
		{
			InteractWithTeleport(enemy, component);
			TeleportCooldownRemaining = 2.5f;
		}
	}

	public void InteractWithAmbient(EnemyAI enemy, IController controller)
	{
		if (Physics.Raycast(enemy.transform.position, enemy.transform.forward, out var hitInfo, InteractRange()))
		{
			if (hitInfo.collider.gameObject.TryGetComponent<DoorLock>(out var component) && DoorCooldownRemaining <= 0f)
			{
				OpenDoorAsEnemy(component);
				DoorCooldownRemaining = 0.7f;
			}
			else if (controller.CanUseEntranceDoors(enemy))
			{
				HandleEntranceDoors(enemy, hitInfo);
			}
		}
	}

	public void OpenDoorAsEnemy(DoorLock door)
	{
		if (!door.Reflect().GetValue<bool>("isDoorOpened"))
		{
			if (door.gameObject.TryGetComponent<AnimatedObjectTrigger>(out var component))
			{
				component.TriggerAnimationNonPlayer(playSecondaryAudios: false, overrideBool: true);
			}
			door.OpenDoorAsEnemyServerRpc();
		}
	}

	private Transform? GetExitPointFromDoor(EntranceTeleport entrance)
	{
		return LethalMenu.doors.Find((EntranceTeleport teleport) => teleport.entranceId == entrance.entranceId && teleport.isEntranceToBuilding != entrance.isEntranceToBuilding)?.entrancePoint;
	}

	public void InteractWithTeleport(EnemyAI enemy, EntranceTeleport teleport)
	{
		AIMovement aIMovement = movement;
		if ((object)aIMovement != null)
		{
			Transform exitPointFromDoor = GetExitPointFromDoor(teleport);
			if ((object)exitPointFromDoor != null)
			{
				aIMovement.SetPosition(exitPointFromDoor.position);
				enemy.EnableEnemyMesh(true, false);
			}
		}
	}

	public void ToggleNoClip()
	{
		NoClipEnabled = !NoClipEnabled;
		movement.SetNoClipMode(NoClipEnabled);
	}

	public void UsePrimarySkill()
	{
		if (!(enemy == null) && !IsAIControlled)
		{
			GetControllerForEnemy(enemy).UsePrimarySkill(enemy);
		}
	}

	public void UseSecondarySkill()
	{
		if (!(enemy == null) && !IsAIControlled)
		{
			GetControllerForEnemy(enemy).UseSecondarySkill(enemy);
		}
	}

	public void OnSecondarySkillHold()
	{
		if (!(enemy == null) && !IsAIControlled)
		{
			GetControllerForEnemy(enemy).OnSecondarySkillHold(enemy);
		}
	}

	public void ReleaseSecondarySkill()
	{
		if (!(enemy == null) && !IsAIControlled)
		{
			GetControllerForEnemy(enemy).ReleaseSecondarySkill(enemy);
		}
	}
}
