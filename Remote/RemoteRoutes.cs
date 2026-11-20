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
		float shownLength = 0f;
		string bestTo = "main";
		foreach (EntranceTeleport door in doors)
		{
			// A door inside the building is a way out; one outside is a way in.
			if (door == null || door.isEntranceToBuilding == inside)
			{
				continue;
			}
			if (!Walk(start.position, door.transform.position, out Vector3[] corners, out float length))
			{
				continue;
			}
			if (length < bestLength)
			{
				bestLength = length;
				shownLength = length;
				best = corners;
				bestTo = door.entranceId == 0 ? "main" : "fire";
			}
		}
		// The Mineshaft: down in the mine, the way out may be up the elevator (it isn't walkable floor, so no route
		// goes through it). Compared by the whole trip: to the elevator, then from its top to the nearest door.
		MineshaftElevatorController lift = RoundManager.Instance != null ? RoundManager.Instance.currentMineshaftElevator : null;
		if (inside && lift != null && lift.elevatorBottomPoint != null && lift.elevatorTopPoint != null && from.y < lift.elevatorTopPoint.position.y - 8f
			&& Walk(start.position, lift.elevatorBottomPoint.position, out Vector3[] toLift, out float liftLength))
		{
			float fromTop = TopToDoor(lift);
			if (liftLength + fromTop < bestLength)
			{
				bestLength = liftLength + fromTop;
				shownLength = liftLength;
				best = toLift;
				bestTo = "elevator";
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
			["to"] = bestTo,
			["main"] = bestTo == "main",
			["m"] = Math.Round(shownLength),
			["pts"] = points
		};
	}

	/// <summary>A complete walking route between two spots (each moved onto the walkable floor), and its length.</summary>
	private static bool Walk(Vector3 from, Vector3 to, out Vector3[] corners, out float length)
	{
		corners = new Vector3[0];
		length = 0f;
		if (!NavMesh.SamplePosition(to, out NavMeshHit end, 4f, NavMesh.AllAreas)
			|| !NavMesh.CalculatePath(from, end.position, NavMesh.AllAreas, Path) || Path.status != NavMeshPathStatus.PathComplete)
		{
			return false;
		}
		corners = Path.corners;
		for (int i = 1; i < corners.Length; i++)
		{
			length += Vector3.Distance(corners[i - 1], corners[i]);
		}
		return true;
	}

	private static MineshaftElevatorController? topToDoorLift;

	private static float topToDoorLength;

	private static float topToDoorAt = -100f;

	/// <summary>Walk from the top of the Mineshaft elevator to the nearest way out (worked out every 30 s).</summary>
	private static float TopToDoor(MineshaftElevatorController lift)
	{
		if (topToDoorLift == lift && Time.unscaledTime - topToDoorAt < 30f)
		{
			return topToDoorLength;
		}
		float bestLength = 1000f; // no way out from up there found: only take the elevator if nothing else works
		if (NavMesh.SamplePosition(lift.elevatorTopPoint.position, out NavMeshHit top, 4f, NavMesh.AllAreas))
		{
			foreach (EntranceTeleport door in doors)
			{
				if (door != null && !door.isEntranceToBuilding && Walk(top.position, door.transform.position, out Vector3[] _, out float length))
				{
					bestLength = Mathf.Min(bestLength, length);
				}
			}
		}
		topToDoorLift = lift;
		topToDoorLength = bestLength;
		topToDoorAt = Time.unscaledTime;
		return bestLength;
	}
}
