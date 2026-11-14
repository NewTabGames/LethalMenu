using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace LethalMenu.Remote;

/// <summary>
/// Draws the website's map. There are no pictures of the facility to download (it is built at random every
/// day), so the map is made from the game's own walkable floor (the nav mesh monsters walk on): every floor
/// triangle is painted onto a grid seen from above, once per landing. Inside and outside are split by which AI
/// nodes each bit of floor is closest to. Each grid cell stores how high the floor there is (1-4, 0 = no
/// floor), so stairs and different floors show in different shades.
///
/// Sent once to devices/{uid}/map (about 5-30 KB); the moving dots come with the normal live state.
/// </summary>
internal static class RemoteMap
{
	/// <summary>Largest side of the grid, in cells. Big moons get bigger cells.</summary>
	private const int MaxCells = 240;

	private const float MinCellSize = 0.75f;

	private static string builtFor;

	private static float nextCheck;

	/// <summary>The map as JSON, waiting to be uploaded (read by the network thread).</summary>
	public static volatile string Latest;

	/// <summary>Main thread. Builds a new map after landing on a moon (once per moon and day).</summary>
	public static void Tick()
	{
		if (Time.unscaledTime < nextCheck)
		{
			return;
		}
		nextCheck = Time.unscaledTime + 2f;
		StartOfRound round = StartOfRound.Instance;
		RoundManager rm = RoundManager.Instance;
		if (round == null || rm == null || round.inShipPhase || !round.shipHasLanded || round.currentLevel == null)
		{
			return;
		}
		if (!rm.dungeonFinishedGeneratingForAllPlayers && round.currentLevel.spawnEnemiesAndScrap)
		{
			return; // the facility's floor isn't there yet
		}
		string key = round.currentLevel.name + "|" + round.randomMapSeed;
		if (key == builtFor)
		{
			return;
		}
		builtFor = key;
		try
		{
			Latest = Build(round, rm).ToString(Formatting.None);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Couldn't build the website map: " + ex.Message);
		}
	}

	public static void Reset()
	{
		builtFor = null;
		Latest = null;
	}

	private static JObject Build(StartOfRound round, RoundManager rm)
	{
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		Vector3[] v = tri.vertices;
		int[] idx = tri.indices;
		Vector3[] inside = Nodes(rm.insideAINodes);
		Vector3[] outside = Nodes(rm.outsideAINodes);

		// Usually the facility sits far below the moon's surface, so a height split is enough; when it isn't,
		// fall back to the nearest AI node.
		float insideTop = inside.Length > 0 ? inside.Max((Vector3 p) => p.y) : float.MinValue;
		float outsideBottom = outside.Length > 0 ? outside.Min((Vector3 p) => p.y) : float.MaxValue;
		bool splitByHeight = inside.Length > 0 && outside.Length > 0 && outsideBottom - insideTop > 30f;
		float splitY = (insideTop + outsideBottom) / 2f;

		List<int> inTris = new List<int>();
		List<int> outTris = new List<int>();
		for (int t = 0; t + 2 < idx.Length; t += 3)
		{
			Vector3 c = (v[idx[t]] + v[idx[t + 1]] + v[idx[t + 2]]) / 3f;
			bool isInside;
			if (inside.Length == 0)
			{
				isInside = false;
			}
			else if (outside.Length == 0)
			{
				isInside = true;
			}
			else if (splitByHeight)
			{
				isInside = c.y < splitY;
			}
			else
			{
				isInside = NearestSq(inside, c) < NearestSq(outside, c);
			}
			(isInside ? inTris : outTris).Add(t);
		}

		JObject map = new JObject
		{
			["moon"] = round.currentLevel.PlanetName,
			["seed"] = round.randomMapSeed,
			["outside"] = Rasterize(v, idx, outTris),
			["inside"] = Rasterize(v, idx, inTris)
		};
		JArray doors = new JArray();
		foreach (EntranceTeleport door in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>())
		{
			if (door == null)
			{
				continue;
			}
			Vector3 p = door.transform.position;
			doors.Add(new JObject
			{
				["x"] = Math.Round(p.x, 1),
				["z"] = Math.Round(p.z, 1),
				["inside"] = !door.isEntranceToBuilding,
				["main"] = door.entranceId == 0
			});
		}
		map["doors"] = doors;
		return map;
	}

	private static Vector3[] Nodes(GameObject[] nodes)
	{
		return (nodes ?? new GameObject[0]).Where((GameObject n) => n != null).Select((GameObject n) => n.transform.position).ToArray();
	}

	private static float NearestSq(Vector3[] points, Vector3 c)
	{
		float best = float.MaxValue;
		for (int i = 0; i < points.Length; i++)
		{
			float d = (points[i] - c).sqrMagnitude;
			if (d < best)
			{
				best = d;
			}
		}
		return best;
	}

	/// <summary>
	/// Paints the triangles onto a grid. Returns {x0, z0, cell, w, h, data} where data is run-length text:
	/// a letter for the value (a = no floor, b-e = floor from low to high) and how many cells in a row have it,
	/// row by row from the lowest z. Null when there is no floor at all.
	/// </summary>
	private static JObject Rasterize(Vector3[] v, int[] idx, List<int> tris)
	{
		if (tris.Count == 0)
		{
			return null;
		}
		float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
		foreach (int t in tris)
		{
			for (int k = 0; k < 3; k++)
			{
				Vector3 p = v[idx[t + k]];
				minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
				minZ = Mathf.Min(minZ, p.z); maxZ = Mathf.Max(maxZ, p.z);
				minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
			}
		}
		minX -= 2f; minZ -= 2f; maxX += 2f; maxZ += 2f;
		float cell = Mathf.Max(MinCellSize, Mathf.Max(maxX - minX, maxZ - minZ) / MaxCells);
		int w = Mathf.CeilToInt((maxX - minX) / cell) + 1;
		int h = Mathf.CeilToInt((maxZ - minZ) / cell) + 1;
		float[] height = new float[w * h];
		for (int i = 0; i < height.Length; i++)
		{
			height[i] = float.NaN;
		}
		void Mark(float x, float z, float y)
		{
			int cx = (int)((x - minX) / cell);
			int cz = (int)((z - minZ) / cell);
			if (cx < 0 || cz < 0 || cx >= w || cz >= h)
			{
				return;
			}
			int i = cz * w + cx;
			if (float.IsNaN(height[i]) || y > height[i])
			{
				height[i] = y;
			}
		}
		foreach (int t in tris)
		{
			Vector3 a = v[idx[t]], b = v[idx[t + 1]], c = v[idx[t + 2]];
			float y = (a.y + b.y + c.y) / 3f;
			// Cells whose middle is inside the triangle...
			int x0 = Mathf.Max(0, (int)((Mathf.Min(a.x, Mathf.Min(b.x, c.x)) - minX) / cell));
			int x1 = Mathf.Min(w - 1, (int)((Mathf.Max(a.x, Mathf.Max(b.x, c.x)) - minX) / cell));
			int z0 = Mathf.Max(0, (int)((Mathf.Min(a.z, Mathf.Min(b.z, c.z)) - minZ) / cell));
			int z1 = Mathf.Min(h - 1, (int)((Mathf.Max(a.z, Mathf.Max(b.z, c.z)) - minZ) / cell));
			float area = Edge(a, b, c.x, c.z);
			if (Mathf.Abs(area) > 1e-5f)
			{
				for (int cz = z0; cz <= z1; cz++)
				{
					float pz = minZ + (cz + 0.5f) * cell;
					for (int cx = x0; cx <= x1; cx++)
					{
						float px = minX + (cx + 0.5f) * cell;
						float w0 = Edge(b, c, px, pz), w1 = Edge(c, a, px, pz), w2 = Edge(a, b, px, pz);
						bool inTri = area > 0 ? (w0 >= 0 && w1 >= 0 && w2 >= 0) : (w0 <= 0 && w1 <= 0 && w2 <= 0);
						if (inTri)
						{
							Mark(px, pz, y);
						}
					}
				}
			}
			// ...plus the edges, so narrow corridors (thinner than a cell) still show up.
			MarkLine(a, b, cell, Mark);
			MarkLine(b, c, cell, Mark);
			MarkLine(c, a, cell, Mark);
		}
		float span = Mathf.Max(1f, maxY - minY);
		StringBuilder data = new StringBuilder();
		char run = '\0';
		int count = 0;
		for (int i = 0; i < height.Length; i++)
		{
			char value = float.IsNaN(height[i]) ? 'a' : (char)('b' + Mathf.Clamp((int)((height[i] - minY) / span * 4f), 0, 3));
			if (value == run)
			{
				count++;
				continue;
			}
			if (count > 0)
			{
				data.Append(run).Append(count);
			}
			run = value;
			count = 1;
		}
		data.Append(run).Append(count);
		return new JObject
		{
			["x0"] = Math.Round(minX, 2),
			["z0"] = Math.Round(minZ, 2),
			["cell"] = Math.Round(cell, 3),
			["w"] = w,
			["h"] = h,
			["data"] = data.ToString()
		};
	}

	private static float Edge(Vector3 a, Vector3 b, float px, float pz)
	{
		return (b.x - a.x) * (pz - a.z) - (b.z - a.z) * (px - a.x);
	}

	private static void MarkLine(Vector3 a, Vector3 b, float cell, Action<float, float, float> mark)
	{
		float length = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
		int steps = Mathf.Max(1, Mathf.CeilToInt(length / (cell * 0.5f)));
		for (int s = 0; s <= steps; s++)
		{
			Vector3 p = Vector3.Lerp(a, b, (float)s / steps);
			mark(p.x, p.z, p.y);
		}
	}
}
