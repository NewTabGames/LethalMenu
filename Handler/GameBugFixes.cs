using System;
using HarmonyLib;

namespace LethalMenu.Handler;

/// <summary>
/// Small fixes for errors the game itself throws, which fill the log (and stutter while they do).
/// </summary>
[HarmonyPatch]
internal static class GameBugFixes
{
	/// <summary>
	/// The ship monitor's dotted "way out" line. Once the ship has taken off the building and its doors are gone, but
	/// if the player on the monitor is still marked as inside (died in there), the game looks for the main entrance
	/// every frame and logs "Main entrance position could not be found" each time. No ship on the ground, no line.
	/// </summary>
	[HarmonyPatch(typeof(ManualCameraRenderer), "SetLineToExitFromRadarTarget")]
	[HarmonyPrefix]
	private static bool NoExitLineAfterTakeoff(ManualCameraRenderer __instance)
	{
		if (StartOfRound.Instance == null || StartOfRound.Instance.shipHasLanded)
		{
			return true;
		}
		if (__instance.lineFromRadarTargetToExit != null)
		{
			__instance.lineFromRadarTargetToExit.enabled = false;
		}
		return false;
	}

	/// <summary>
	/// The game keeps its list of which background sound to play next from the last moon, so on a moon with fewer
	/// sounds it can pick one past the end ("IndexOutOfRangeException" in PlayAmbienceClipLocal). Skip that one sound.
	/// </summary>
	[HarmonyPatch(typeof(SoundManager), "PlayAmbienceClipLocal")]
	[HarmonyPrefix]
	private static bool SkipMissingAmbience(SoundManager __instance, int soundType, int clipIndex, bool playInsanitySounds)
	{
		LevelAmbienceLibrary? sounds = __instance.currentLevelAmbience;
		if (sounds == null)
		{
			return false;
		}
		Array? clips = soundType switch
		{
			0 => playInsanitySounds ? sounds.insideAmbienceInsanity : sounds.insideAmbience,
			1 => playInsanitySounds ? sounds.outsideAmbienceInsanity : sounds.outsideAmbience,
			2 => playInsanitySounds ? sounds.shipAmbienceInsanity : sounds.shipAmbience,
			_ => null
		};
		return clips == null || (clipIndex >= 0 && clipIndex < clips.Length);
	}
}
