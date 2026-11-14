using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;
using Steamworks;
using Steamworks.Data;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class AntiKick : Cheat
{
	public static List<ulong> HostKickedPlayerList = new List<ulong>();

	private static bool HostQuit = false;

	private static bool HostKicked = false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesServerRpc")]
	public static bool SendNewPlayerValuesServerRpc(PlayerControllerB __instance, ulong newPlayerSteamId)
	{
		if (Hack.AntiKick.IsEnabled() && HostKicked)
		{
			__instance.sentPlayerValues = true;
			ulong[] array = __instance.playersManager.allPlayerScripts.Select((PlayerControllerB p) => p.playerSteamId).ToArray();
			array[__instance.playerClientId] = SteamClient.SteamId;
			__instance.Reflect().Invoke("SendNewPlayerValuesClientRpc", array);
			HostKicked = false;
			return false;
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
	public static void Disconnect(GameNetworkManager __instance)
	{
		if (GameNetworkManager.Instance.disconnectReason == 1)
		{
			if (!HostQuit)
			{
				HostKickedPlayerList.Add(Settings.s_lobbyid);
			}
			HostQuit = false;
		}
		if (GameNetworkManager.Instance.disconnectReason == 3)
		{
			HostKickedPlayerList.Add(Settings.s_lobbyid);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(StartOfRound), "OnClientDisconnect")]
	public static void OnClientDisconnect(StartOfRound __instance, ulong clientId)
	{
		if (StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out var value) && value == 0 && !GameNetworkManager.Instance.isDisconnecting)
		{
			HostQuit = true;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameNetworkManager), "JoinLobby")]
	public static void JoinLobby(GameNetworkManager __instance, Lobby lobby, SteamId id)
	{
		if (HostKickedPlayerList.Contains(lobby.Owner.Id))
		{
			HostKicked = true;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameNetworkManager), "StartClient")]
	public static void StartClient(GameNetworkManager __instance, SteamId id)
	{
		if (HostKickedPlayerList.Contains(id))
		{
			HostKicked = true;
		}
	}
}
