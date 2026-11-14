using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler;

public static class EnemyAIExtensions
{
	/// <summary>
	/// What players call each monster. The game names every monster twice, and neither is always the name
	/// players know: the type's file name ("SpringMan", also this menu's monster list) and its "enemy name"
	/// ("Spring"). Both are listed, written without spaces or capitals (see <see cref="Key"/>).
	/// Internal names are from the game's own files; the names shown are the ones chosen for this menu.
	/// </summary>
	private static readonly System.Collections.Generic.Dictionary<string, string> ShownNames = new System.Collections.Generic.Dictionary<string, string>
	{
		{ "baboonhawk", "Baboon Hawk" },
		{ "blob", "Hygrodere" },
		{ "bushwolf", "Kidnapper Fox" },
		{ "butler", "Butler" },
		{ "butlerbees", "Mask Hornets" },
		{ "cadaverbloom", "Cadaver Bloom" },
		{ "cadavergrowths", "Cadaver Growths" },
		{ "cavedweller", "ManEater" },
		{ "maneater", "ManEater" },
		{ "centipede", "Snare Flea" },
		{ "claysurgeon", "Barber" },
		{ "crawler", "Thumper" },
		{ "docilelocustbees", "Roaming Locusts" },
		{ "doublewing", "Manticoil" },
		{ "manticoil", "Manticoil" },
		{ "flowersnake", "Tulip Snake" },
		{ "tulipsnake", "Tulip Snake" },
		{ "redlocustbees", "Locust Bees" },
		{ "dressgirl", "Ghost Girl" },
		{ "girl", "Ghost Girl" },
		{ "flowerman", "Bracken" },
		{ "forestgiant", "Forest Keeper" },
		{ "giantkiwi", "Big Bird" },
		{ "giantsapsucker", "Big Bird" },
		{ "hoarderbug", "Loot Bug" },
		{ "hoardingbug", "Loot Bug" },
		{ "jester", "Jester" },
		{ "lassoman", "Lasso Man" },
		{ "lasso", "Lasso Man" },
		{ "maskedplayerenemy", "Masked" },
		{ "masked", "Masked" },
		{ "mouthdog", "Eyeless Dog" },
		{ "eyelessdog", "Eyeless Dog" },
		{ "nutcracker", "Nutcracker" },
		{ "puffer", "Spore Lizard" },
		{ "puma", "Feiopar" },
		{ "feiopar", "Feiopar" },
		{ "radmech", "Old Bird" },
		{ "sandspider", "Bunker Spider" },
		{ "bunkerspider", "Bunker Spider" },
		{ "sandworm", "Earth Leviathan" },
		{ "earthleviathan", "Earth Leviathan" },
		{ "springman", "CoilHead" },
		{ "spring", "CoilHead" },
		{ "stingray", "Gunk Fish" },
		{ "redpillenemytype", "Red Pill" },
		{ "redpill", "Red Pill" }
	};

	/// <summary>"Hoarding bug" / "HoarderBug" / "Flower Man" → "hoardingbug" / "hoarderbug" / "flowerman".</summary>
	private static string Key(string name)
	{
		System.Text.StringBuilder key = new System.Text.StringBuilder(name.Length);
		foreach (char c in name)
		{
			if (char.IsLetterOrDigit(c))
			{
				key.Append(char.ToLowerInvariant(c));
			}
		}
		return key.ToString();
	}

	/// <summary>The name to show for an internal monster name (anything not listed is shown as-is).</summary>
	public static string ShownName(string internalName)
	{
		return !string.IsNullOrEmpty(internalName) && ShownNames.TryGetValue(Key(internalName), out string shown) ? shown : internalName;
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

	/// <summary>
	/// Whether it's outside right now, from where it actually is. The game's own isOutside comes from the kind of
	/// monster, so one spawned on the other side (or seen from a client) was listed in the wrong place. The
	/// facility is always far below the moon's surface; the game uses the same height test.
	/// </summary>
	public static bool IsOutsideNow(this EnemyAI enemy)
	{
		return enemy.transform.position.y > -80f;
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
