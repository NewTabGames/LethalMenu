using GameNetcodeStuff;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal static class ControlUtil
{
	/// <summary>
	/// Who a controlled monster's skill should go for: the player picked with T / the website if there is one
	/// (which can be yourself), otherwise the closest living player other than you. Null if nobody is within
	/// range (and in sight, when asked) - a picked target that is out of range is not swapped for someone else.
	/// </summary>
	public static PlayerControllerB? GetNearestPlayer(EnemyAI enemy, float maxDistance, bool requireLineOfSight = true)
	{
		if (enemy == null || StartOfRound.Instance == null)
		{
			return null;
		}
		PlayerControllerB? best = null;
		float bestDistance = maxDistance;
		Vector3 origin = enemy.eye != null ? enemy.eye.position : enemy.transform.position + Vector3.up;
		PlayerControllerB? chosen = Cheats.EnemyControl.ControlledEnemy == enemy ? Cheats.EnemyControl.ValidChosenTarget : null;
		if (chosen != null)
		{
			bool inRange = Vector3.Distance(enemy.transform.position, chosen.transform.position) <= maxDistance;
			bool inSight = !requireLineOfSight || !Physics.Linecast(origin, chosen.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
			return inRange && inSight ? chosen : null;
		}
		foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
		{
			if (player == null || !player.isPlayerControlled || player.isPlayerDead || player == LethalMenu.localPlayer)
			{
				continue;
			}
			float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
			if (distance > bestDistance)
			{
				continue;
			}
			if (requireLineOfSight && Physics.Linecast(origin, player.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
			{
				continue;
			}
			best = player;
			bestDistance = distance;
		}
		return best;
	}
}
