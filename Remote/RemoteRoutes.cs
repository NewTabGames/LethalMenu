using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace LethalMenu.Remote;

/// <summary>
/// The website map's "way out" line: the real walking route from a player to the nearest door on their side
/// (inside: out of the building through the main entrance or a fire exit; outside: the way in). Worked out on the
/// game's own walkable floor (the navmesh monsters use), so it follows stairs and floors properly. The website only
/// has a flat picture of the floor and falls back to guessing from that when a route isn't sent.
/// </summary>
internal static class RemoteRoutes
{
	private sealed class Cached
	{
		public Vector3 From;

		public bool Inside;

		public float At;

		public JObject? Route;
	}

	private static readonly Dictionary<int, Cached> Cache = new Dictionary<int, Cached>();

	private static EntranceTeleport[] doors = new EntranceTeleport[0];

	private static float doorsFoundAt = -100f;

	private static readonly NavMeshPath Path = new NavMeshPath();

	/// <summary>The route for this player (by player index), or null when there is none to show.</summary>
	internal static JObject? For(PlayerControllerB player, int index)
	{
		if (player == null || player.isPlayerDead || !player.isPlayerControlled)
		{
			Cache.Remove(index);
			return null;
		}
		Vector3 from = player.transform.position;
		bool inside = player.isInsideFactory;
		// Only work it out again once they've moved a metre (or every 3 s): twice a second for every player is a lot
		// of route finding for nothing while they stand still.
		if (Cache.TryGetValue(index, out Cached cached) && cached.Inside == inside && Time.unscaledTime - cached.At < 3f && (cached.From - from).sqrMagnitude < 1f)
		{
			return cached.Route;
		}
		JObject? route = null;
		try
		{
			route = Compute(from, inside);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Way-out route: " + ex.Message);
		}
		Cache[index] = new Cached { From = from, Inside = inside, At = Time.unscaledTime, Route = route };
		return route;
	}

	private static JObject? Compute(Vector3 from, bool inside)
	{
		if (Time.unscaledTime - doorsFoundAt > 10f || Array.Exists(doors, (EntranceTeleport d) => d == null))
		{
			doors = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
			doorsFoundAt = Time.unscaledTime;
		}
		if (!NavMesh.SamplePosition(from, out NavMeshHit start, 4f, NavMesh.AllAreas))
		{
			return null;
		}
		Vector3[]? best = null;
		float bestLength = float.MaxValue;
		bool bestMain = false;
		foreach (EntranceTeleport door in doors)
		{
			// A door inside the building is a way out; one outside is a way in.
			if (door == null || door.isEntranceToBuilding == inside)
			{
				continue;
			}
			if (!NavMesh.SamplePosition(door.transform.position, out NavMeshHit end, 4f, NavMesh.AllAreas))
			{
				continue;
			}
			if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, Path) || Path.status != NavMeshPathStatus.PathComplete)
			{
				continue;
			}
			Vector3[] corners = Path.corners;
			float length = 0f;
			for (int i = 1; i < corners.Length; i++)
			{
				length += Vector3.Distance(corners[i - 1], corners[i]);
			}
			if (length < bestLength)
			{
				bestLength = length;
				best = corners;
				bestMain = door.entranceId == 0;
			}
		}
		if (best == null || best.Length < 2)
		{
			return null;
		}
		JArray points = new JArray();
		foreach (Vector3 p in best)
		{
			points.Add(new JArray(Math.Round(p.x * 2) / 2, Math.Round(p.z * 2) / 2, Math.Round(p.y)));
		}
		return new JObject
		{
			["main"] = bestMain,
			["m"] = Math.Round(bestLength),
			["pts"] = points
		};
	}
}
