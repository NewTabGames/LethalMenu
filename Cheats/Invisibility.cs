using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class Invisibility : Cheat
{
	public override void Update()
	{
		if (!(LethalMenu.localPlayer == null) && Hack.Invisibility.IsEnabled())
		{
			Vector3 vector = (StartOfRound.Instance.shipHasLanded ? StartOfRound.Instance.notSpawnedPosition.position : Vector3.zero);
			LethalMenu.localPlayer.Reflect().Invoke("UpdatePlayerPositionRpc", vector, true, false, false, true);
		}
	}
}
