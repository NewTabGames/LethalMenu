using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>Rules for every monster you drive.</summary>
[HarmonyPatch]
internal static class ControlledEnemyPatches
{
	// Some monsters hand themselves to another player by their own AI (the Feiopar gives itself back to the host,
	// or to the player it stalks), which took a monster away from you mid-control. Not while you drive it.
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.ChangeOwnershipOfEnemy))]
	public static bool ChangeOwnershipOfEnemy(EnemyAI __instance, ulong newOwnerClientId)
	{
		return !Control.IsDrivenByYou(__instance) || LethalMenu.localPlayer == null || newOwnerClientId == LethalMenu.localPlayer.actualClientId;
	}
}

/// <summary>
/// Every monster runs its AI a few times a second (DoAIInterval: pick a target, choose where to go, switch modes,
/// leap, fly off, hide...). While you drove one it kept doing all of that on top of your steering, which is what
/// broke so many of them: the Old Bird flew off, the Bunker Spider climbed walls, the Tulip Snake leapt at people
/// by itself, monsters switched modes under you. While you drive a monster, its AI step is skipped (only the part
/// that sends its position to the other players still runs). With AI driving on (F9) it runs as normal.
/// </summary>
[HarmonyPatch]
internal static class ControlledEnemyAIPause
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

	private static bool Prefix(EnemyAI __instance)
	{
		if (!Control.IsDrivenByYou(__instance))
		{
			return true;
		}
		__instance.SyncPositionToClients();
		return false;
	}
}
