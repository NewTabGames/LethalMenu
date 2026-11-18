using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;
using UnityEngine.AI;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Tulip snake. Left click leaps where you look; land on a player and it clings to them like normal.
/// While clinging: left click lets go, right click flaps its wings (with two or more snakes on someone,
/// flapping lifts them off the ground, as in the normal game).
/// </summary>
internal class FlowerSnakeController : IEnemyController<FlowerSnakeEnemy>, IController
{
	/// <summary>How long one flap lasts before the wings stop again (the game's own flaps last 4-9 s).</summary>
	private const float FlapSeconds = 8f;

	private static readonly System.Reflection.MethodInfo SetFlappingLocal = AccessTools.Method(typeof(FlowerSnakeEnemy), "SetFlappingLocalClient");

	private static bool Leaping(FlowerSnakeEnemy enemy)
	{
		return enemy.Reflect().GetValue<bool>("leaping") || enemy.Reflect().GetValue<bool>("fallingFromLeap");
	}

	private static bool Clinging(FlowerSnakeEnemy enemy)
	{
		return enemy.clingingToPlayer != null;
	}

	// The game moves it itself while it leaps, falls back down or rides on someone, and for a moment after it
	// lets go (it's put back on the floor then). Steering in between used to drag it back to where it started.
	public bool IsAbleToMove(FlowerSnakeEnemy enemy)
	{
		return !Leaping(enemy) && !Clinging(enemy) && !enemy.inSpecialAnimation && Time.realtimeSinceStartup - enemy.Reflect().GetValue<float>("timeOfLastCling") > 0.3f;
	}

	public bool IsAbleToRotate(FlowerSnakeEnemy enemy)
	{
		return IsAbleToMove(enemy);
	}

	private float nextLeapSync;

	public void Update(FlowerSnakeEnemy enemy, bool isAIControlled)
	{
		// Only the host can show the leap to everyone. For anyone else the leap ran on their screen only, and the
		// game stops sending the snake's position during a leap, so the others saw it sit still and then jump to
		// where it landed (it could never land on them). Send the position along the arc instead.
		if (isAIControlled || enemy == null || enemy.IsServer || !Leaping(enemy))
		{
			return;
		}
		if (Time.time >= nextLeapSync)
		{
			nextLeapSync = Time.time + 0.05f;
			enemy.SyncPositionToClients();
		}
	}

	public void UsePrimarySkill(FlowerSnakeEnemy enemy)
	{
		if (Clinging(enemy))
		{
			LetGo(enemy);
			return;
		}
		if (Leaping(enemy))
		{
			return;
		}
		// Straight ahead, where you face. The game gives the leap its arc.
		Vector3 direction = enemy.transform.forward;
		direction.y = 0f;
		direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
		enemy.StartLeapOnLocalClient(direction);
		if (enemy.IsServer)
		{
			enemy.StartLeapClientRpc(direction);
		}
	}

	public void UseSecondarySkill(FlowerSnakeEnemy enemy)
	{
		if (!Clinging(enemy))
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Leap onto a player first, then flap");
			return;
		}
		if (!enemy.IsServer)
		{
			// The game only lets the host start a flap for everyone.
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Only the host can make a tulip snake flap");
			return;
		}
		bool flap = !enemy.Reflect().GetValue<bool>("flapping");
		// The game's own timer switches flapping on and off; set it up so it agrees with this flap.
		enemy.Reflect().SetValue("clingingPlayerFlapInterval", flap ? 1f : 0f);
		enemy.Reflect().SetValue("flapIntervalTimeOffset", flap ? FlapSeconds : Random.Range(4f, 9f));
		SetFlappingLocal.Invoke(enemy, new object[2] { flap, true });
		enemy.SetFlappingClientRpc(flap);
		if (flap && enemy.clingingToPlayer.enemiesOnPerson < 2)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Flapping. It takes two or more snakes on someone to lift them");
		}
	}

	/// <summary>
	/// Lets go of the player. Asked with no player id (-1): the game then makes every client let go, including the
	/// player it was on. With their real id their own game would have kept the snake on them.
	/// </summary>
	private static void LetGo(FlowerSnakeEnemy enemy)
	{
		enemy.StopClingingServerRpc(-1);
	}

	public string GetPrimarySkillName(FlowerSnakeEnemy enemy)
	{
		return Clinging(enemy) ? "Let go" : "Leap";
	}

	public string GetSecondarySkillName(FlowerSnakeEnemy enemy)
	{
		if (!Clinging(enemy))
		{
			return "Flap (while on a player)";
		}
		return enemy.Reflect().GetValue<bool>("flapping") ? "Stop flapping" : "Flap wings";
	}

	public bool CanUseEntranceDoors(FlowerSnakeEnemy _)
	{
		return true;
	}

	public float InteractRange(FlowerSnakeEnemy _)
	{
		return 5f;
	}
}

[HarmonyPatch]
internal static class FlowerSnakeAIPatches
{
	private static readonly AccessTools.FieldRef<EnemyAI, int> AgentMask = AccessTools.FieldRefAccess<EnemyAI, int>("agentMask");

	/// <summary>
	/// On a non-host client, StopLeapOnLocalClient(landOnGround: true) uses overrideLandingPosition instead of
	/// sampling the NavMesh, which defaults to (0, 0, 0). That pulled the driven snake toward world origin and
	/// marked indoor snakes as outside. Compute the landing position on the driver the same way the host does.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(FlowerSnakeEnemy), nameof(FlowerSnakeEnemy.StopLeapOnLocalClient))]
	public static void StopLeapOnLocalClientPrefix(FlowerSnakeEnemy __instance, bool landOnGround, ref Vector3 overrideLandingPosition)
	{
		if (landOnGround && !__instance.IsServer && overrideLandingPosition == default && Control.IsDrivenByYou(__instance))
		{
			Vector3 pos = RoundManager.Instance.GetNavMeshPosition(__instance.transform.position, default(NavMeshHit), 15f, AgentMask(__instance));
			if (!RoundManager.Instance.GotNavMeshPositionResult)
			{
				pos = __instance.transform.position;
			}
			overrideLandingPosition = pos;
		}
	}
}

