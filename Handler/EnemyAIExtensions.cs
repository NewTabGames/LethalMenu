using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler;

public static class EnemyAIExtensions
{
	/// <summary>The game's internal monster names that differ from what players call them.</summary>
	private static readonly System.Collections.Generic.Dictionary<string, string> ShownNames = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "Crawler", "Thumper" }
	};

	/// <summary>The name to show for an internal monster name (anything not listed is shown as-is).</summary>
	public static string ShownName(string internalName)
	{
		return internalName != null && ShownNames.TryGetValue(internalName.Trim(), out string shown) ? shown : internalName;
	}

	public static string ShownName(this EnemyAI enemy)
	{
		if (enemy == null)
		{
			return "Monster";
		}
		return ShownName(enemy.enemyType != null ? enemy.enemyType.enemyName : enemy.name);
	}

	public static EnemyHandler Handle(this EnemyAI enemy)
	{
		return EnemyHandler.GetHandler(enemy);
	}

	public static bool IsBehaviourState(this EnemyAI enemy, Enum state)
	{
		return enemy.currentBehaviourStateIndex == Convert.ToInt32(state);
	}

	public static void SetBehaviourState(this EnemyAI enemy, Enum state)
	{
		if (!enemy.IsBehaviourState(state))
		{
			enemy.SwitchToBehaviourServerRpc(Convert.ToInt32(state));
		}
	}

	public static GrabbableObject? FindNearbyItem(this EnemyAI enemy, float grabRange = 1f)
	{
		Collider[] array = Physics.OverlapSphere(enemy.transform.position, grabRange);
		foreach (Collider collider in array)
		{
			if (collider.TryGetComponent<GrabbableObject>(out var component) && component.TryGetComponent<NetworkObject>(out var _))
			{
				return component;
			}
		}
		return null;
	}

	public static bool IsOutside(this EnemyAI enemy)
	{
		if (RoundManager.Instance == null || RoundManager.Instance.outsideAINodes == null || RoundManager.Instance.insideAINodes == null || RoundManager.Instance.outsideAINodes.Length == 0 || RoundManager.Instance.insideAINodes.Length == 0)
		{
			return false;
		}
		return RoundManager.Instance.outsideAINodes.Min((GameObject n) => Vector3.Distance(enemy.serverPosition, n.transform.position)) < RoundManager.Instance.insideAINodes.Min((GameObject n) => Vector3.Distance(enemy.serverPosition, n.transform.position));
	}
}
