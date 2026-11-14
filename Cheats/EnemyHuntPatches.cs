using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace LethalMenu.Cheats;

/// <summary>
/// Runs right after every monster's own AI step (each monster type has its own DoAIInterval). If that monster is
/// walking to someone (Hunt), point it back at them, so its normal wandering or searching can't pull it away.
/// </summary>
[HarmonyPatch]
internal static class EnemyHuntPatches
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		System.Type[] types;
		try
		{
			types = typeof(EnemyAI).Assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			types = ex.Types.Where((System.Type t) => t != null).ToArray();
		}
		return types
			.Where((System.Type t) => typeof(EnemyAI).IsAssignableFrom(t))
			.Select((System.Type t) => t.GetMethod("DoAIInterval", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			.Where((MethodInfo m) => m != null && !m.IsAbstract)
			.Cast<MethodBase>();
	}

	/// <summary>
	/// Aggro hunt for an Eyeless Dog. Far away it just walks toward the player like a normal hunt. Inside
	/// AggroRange it is enraged with the game's own "heard a loud noise right there" reaction (so it howls and
	/// sprints), and for as long as it is enraged its noise position is kept on the player, which is what makes
	/// it lunge when it gets within 4m. The bite itself is the game's normal collision kill.
	/// </summary>
	private static void SteerAggroDog(MouthDogAI dog, EnemyHunt.Hunt hunt, NavMeshAgent agent)
	{
		if (!EnemyHunt.SameArea(dog, hunt.Target))
		{
			return;
		}
		Vector3 target = hunt.Target.transform.position;
		float distance = Vector3.Distance(dog.transform.position, target);
		int state = dog.currentBehaviourStateIndex;
		if (state < 2)
		{
			if (distance > EnemyHunt.AggroRange)
			{
				agent.SetDestination(target); // still far: walk
				return;
			}
			if (Time.time >= hunt.NextEnrage)
			{
				hunt.NextEnrage = Time.time + 2f;
				Traverse.Create(dog).Method("EnrageDogOnLocalClient", target, distance, false, true).GetValue();
			}
			return;
		}
		if (state == 2)
		{
			// Enraged and sprinting: keep it locked on where the player is now, and don't let it calm down.
			dog.suspicionLevel = 12;
			Traverse traverse = Traverse.Create(dog);
			traverse.Field("noisePositionGuess").SetValue(target);
			if (!traverse.Field("inLunge").GetValue<bool>())
			{
				dog.SetDestinationToPosition(target);
				agent.SetDestination(target);
			}
		}
		// state 3 = mid-lunge: leave it alone, it comes back to state 2 by itself.
	}

	[HarmonyPostfix]
	private static void Steer(EnemyAI __instance)
	{
		if (EnemyHunt.Hunts.Count == 0)
		{
			return;
		}
		EnemyHunt.Hunt hunt = EnemyHunt.Find(__instance);
		if (hunt == null || hunt.Target == null || !__instance.IsOwner || __instance.isEnemyDead || EnemyControl.IsDrivenByYou(__instance))
		{
			return;
		}
		NavMeshAgent agent = __instance.agent;
		if (agent == null || !agent.enabled || !agent.isOnNavMesh)
		{
			return;
		}
		// Only the path is changed: no target player, no chase state, and the AI's own idea of where it was
		// going is left untouched, so when the hunt ends it simply carries on with that.
		if (hunt.Aggro && __instance is MouthDogAI dog)
		{
			SteerAggroDog(dog, hunt, agent);
			return;
		}
		if (hunt.Kill && EnemyHunt.SameArea(__instance, hunt.Target))
		{
			// Kill hunt: once close, make it angry at the player (the same as "Anger at player") so its own AI
			// attacks with its normal damage. Repeated every few seconds in case it calms down or switches target.
			bool onTarget = EnemyHunt.IsChasing(__instance) && (__instance.targetPlayer == null || __instance.targetPlayer == hunt.Target);
			if (!onTarget && Time.time >= hunt.NextEnrage && Vector3.Distance(__instance.transform.position, hunt.Target.transform.position) <= EnemyHunt.KillAngerRange)
			{
				hunt.NextEnrage = Time.time + 3f;
				try
				{
					Handler.EnemyAIExtensions.Handle(__instance).TargetPlayer(hunt.Target);
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning("[LethalMenu] Kill hunt anger: " + ex.Message);
				}
			}
		}
		if (hunt.Persistent)
		{
			// Keeps going at them for as long as the hunt is on. It waits while they are in the other area
			// (inside vs outside), and if it is chasing someone else its chase is pointed at the hunted player.
			if (!EnemyHunt.SameArea(__instance, hunt.Target) || Vector3.Distance(__instance.transform.position, hunt.Target.transform.position) < 2f)
			{
				return;
			}
			if (EnemyHunt.IsChasing(__instance))
			{
				if (__instance.targetPlayer != null && __instance.targetPlayer != hunt.Target)
				{
					__instance.targetPlayer = hunt.Target;
				}
				return;
			}
			agent.SetDestination(hunt.Target.transform.position);
			return;
		}
		if (EnemyHunt.IsChasing(__instance))
		{
			return;
		}
		agent.SetDestination(hunt.Target.transform.position);
	}
}
