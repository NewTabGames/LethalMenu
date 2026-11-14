using System.Collections;
using System.Linq;
using UnityEngine;

namespace LethalMenu.Handler;

public static class SandSpiderAIExtensions
{
	/// <summary>A living Bunker Spider on the map, or null. The game can only place webs through a spider.</summary>
	public static SandSpiderAI FindSpider()
	{
		return LethalMenu.enemies.OfType<SandSpiderAI>().FirstOrDefault((SandSpiderAI s) => s != null && !s.isEnemyDead && s.IsSpawned)
			?? Object.FindObjectsOfType<SandSpiderAI>().FirstOrDefault((SandSpiderAI s) => s != null && !s.isEnemyDead && s.IsSpawned);
	}

	/// <summary>
	/// Places one web at <paramref name="position"/>: anchored on the floor there and stretched to a nearby wall,
	/// like the spider's own webs (a web whose start and end are the same point is invisible and does nothing).
	/// Only works when this client owns the spider; use <see cref="SpawnWebs"/> to take ownership first.
	/// </summary>
	public static int SpawnWeb(this SandSpiderAI spider, Vector3 position)
	{
		int mask = StartOfRound.Instance.collidersAndRoomMask;
		if (!Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out var floor, 10f, mask))
		{
			return -1;
		}
		Vector3 start = floor.point + Vector3.up * 0.2f;
		for (int attempt = 0; attempt < 16; attempt++)
		{
			Vector3 direction = Random.onUnitSphere;
			direction.y = Random.Range(-0.25f, 0.35f);
			if (Physics.Raycast(start + Vector3.up * 0.4f, direction.normalized, out var wall, 7f, mask) && wall.distance >= 1.5f)
			{
				spider.SpawnWebTrapServerRpc(start, wall.point);
				return spider.webTraps.Count - 1;
			}
		}
		return -1;
	}

	/// <summary>Takes over the spider if needed (placing webs is owner-only), then places <paramref name="count"/> webs.</summary>
	public static void SpawnWebs(this SandSpiderAI spider, Vector3 position, int count)
	{
		LethalMenu.Instance.StartCoroutine(SpawnWebsWhenOwner(spider, position, count));
	}

	private static IEnumerator SpawnWebsWhenOwner(SandSpiderAI spider, Vector3 position, int count)
	{
		if (!spider.IsOwner)
		{
			spider.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
			float waited = 0f;
			while (spider != null && !spider.IsOwner && waited < 3f)
			{
				waited += Time.deltaTime;
				yield return null;
			}
		}
		if (spider == null || !spider.IsOwner)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Could not take over the Bunker Spider to place webs");
			yield break;
		}
		int placed = 0;
		for (int i = 0; i < count; i++)
		{
			if (spider.SpawnWeb(position) >= 0)
			{
				placed++;
			}
		}
		if (placed == 0)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No room for webs there (needs walls within 7m)");
		}
	}

	/// <summary>
	/// Breaking a web removes it from the spider's list and renumbers the ones after it, so go from the last web
	/// to the first. The host tells everyone directly (no spider chase); anyone else asks the host, which needs a
	/// real player number (-1 crashed the game's code), so the spider will come after you.
	/// </summary>
	public static void BreakAllWebs(this SandSpiderAI spider)
	{
		if (spider == null || spider.webTraps == null)
		{
			return;
		}
		int me = StartOfRound.Instance != null && LethalMenu.localPlayer != null ? System.Array.IndexOf(StartOfRound.Instance.allPlayerScripts, LethalMenu.localPlayer) : -1;
		for (int i = spider.webTraps.Count - 1; i >= 0; i--)
		{
			SandSpiderWebTrap web = spider.webTraps[i];
			if (web == null || web.centerOfWeb == null)
			{
				continue;
			}
			if (spider.IsServer)
			{
				spider.BreakWebClientRpc(web.centerOfWeb.position, i);
			}
			else if (me >= 0)
			{
				spider.BreakWebServerRpc(i, me);
			}
		}
	}
}
