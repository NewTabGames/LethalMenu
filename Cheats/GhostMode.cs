using GameNetcodeStuff;
using HarmonyLib;

namespace LethalMenu.Cheats;

[HarmonyPatch(typeof(EnemyAI), "PlayerIsTargetable")]
internal class GhostMode : Cheat
{
	[HarmonyPrefix]
	public static bool Prefix(PlayerControllerB playerScript, bool cannotBeInShip = false, bool overrideInsideFactoryCheck = false)
	{
		if (Hack.GhostMode.IsEnabled() && LethalMenu.localPlayer != null && LethalMenu.localPlayer.playerClientId == playerScript.playerClientId)
		{
			return false;
		}
		return true;
	}
}
