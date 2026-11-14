using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class GodMode : Cheat
{
	public override void Update()
	{
		if (Hack.GodMode.IsEnabled() && !(LethalMenu.localPlayer == null))
		{
			LethalMenu.localPlayer.health = 100;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
	public static bool PrefixDamagePlayer(int damageNumber, bool hasDamageSFX = true, bool callRPC = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0, bool fallDamage = false, Vector3 force = default(Vector3))
	{
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
	public static bool PrefixKillPlayer(Vector3 bodyVelocity, bool spawnBody = true, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown, int deathAnimation = 0)
	{
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FlowermanAI), "KillPlayerAnimationServerRpc")]
	public static bool PrefixFlowerman(int playerObjectId)
	{
		if (LethalMenu.localPlayer == null || playerObjectId != (int)LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ForestGiantAI), "GrabPlayerServerRpc")]
	public static bool PrefixGiant(int playerId)
	{
		if (LethalMenu.localPlayer == null || playerId != (int)LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(JesterAI), "KillPlayerServerRpc")]
	public static bool PrefixJester(int playerId)
	{
		if (LethalMenu.localPlayer == null || playerId != (int)LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaskedPlayerEnemy), "KillPlayerAnimationServerRpc")]
	public static bool PrefixMaskedPlayer(int playerObjectId)
	{
		if (LethalMenu.localPlayer == null || playerObjectId != (int)LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MouthDogAI), "OnCollideWithPlayer")]
	public static bool PrefixDog(MouthDogAI __instance, Collider other)
	{
		if (__instance == null || other == null)
		{
			return true;
		}
		PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
		if (playerControllerB == null || LethalMenu.localPlayer == null || playerControllerB.playerClientId != LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CentipedeAI), "OnCollideWithPlayer")]
	public static bool PrefixCentipede(CentipedeAI __instance, Collider other)
	{
		if (__instance == null || other == null)
		{
			return true;
		}
		PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
		if (playerControllerB == null || LethalMenu.localPlayer == null || playerControllerB.playerClientId != LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RadMechAI), "OnCollideWithPlayer")]
	public static bool PrefixRadMechKill(RadMechAI __instance, Collider other)
	{
		if (__instance == null || other == null)
		{
			return true;
		}
		PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
		if (playerControllerB == null || LethalMenu.localPlayer == null || playerControllerB.playerClientId != LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(BushWolfEnemy), "OnCollideWithPlayer")]
	public static bool PrefixBushWolfEnemy(BushWolfEnemy __instance, Collider other)
	{
		if (__instance == null || other == null)
		{
			return true;
		}
		PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
		if (playerControllerB == null || LethalMenu.localPlayer == null || playerControllerB.playerClientId != LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CaveDwellerAI), "OnCollideWithPlayer")]
	public static bool PrefixCaveDwellerAI(CaveDwellerAI __instance, Collider other)
	{
		if (__instance == null || other == null)
		{
			return true;
		}
		PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
		if (playerControllerB == null || LethalMenu.localPlayer == null || playerControllerB.playerClientId != LethalMenu.localPlayer.playerClientId)
		{
			return true;
		}
		return !Hack.GodMode.IsEnabled();
	}
}
