using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace LethalMenu.Handler;

public static class PlayerHandlerExtensions
{
	public static PlayerHandler Handle(this PlayerControllerB player)
	{
		return new PlayerHandler(player);
	}

	public static bool IsRealPlayer(this PlayerControllerB player)
	{
		PlayerControllerB playerControllerB = LethalMenu.players.FirstOrDefault((PlayerControllerB p) => p != null && IsHost(p));
		if (Vector3.Distance(player.serverPlayerPosition, StartOfRound.Instance.notSpawnedPosition.position) <= 10f || (playerControllerB != null && playerControllerB != player && player.playerSteamId == playerControllerB.playerSteamId) || player.playerSteamId == 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsHost(this PlayerControllerB player)
	{
		return (player != null && player.IsHost) || player.actualClientId == 0;
	}
}
