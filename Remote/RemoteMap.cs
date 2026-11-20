using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

	/// <summary>Which landing Latest is the map of.</summary>
	private static volatile string readyFor;

	private static float nextCheck;

	/// <summary>The map as JSON, waiting to be uploaded (read by the network thread).</summary>
	public static volatile string Latest;

	/// <summary>A map being worked out on a background thread right now.</summary>
	private static volatile bool building;

	/// <summary>Everything the background part needs, read from the game on the main thread.</summary>
	private sealed class Input
	{
		public string Key;
		public string Moon;
		public int Seed;
		public Vector3[] V;
		public int[] Idx;
		public Vector3[] Inside;
		public Vector3[] Outside;
		public Vector3? Ship;
		public JObject Elevator;
		public JArray Doors;
	}

	/// <summary>A map (no walk floor outside: the Company) waiting for its ground scan, done a few rows a frame.</summary>
	private sealed class Scan
	{
		public string Key;
		public JObject Map;
		public string Note;
		public string Rest;
		public Bounds Area;
		public float Cell;
		public int W;
		public int H;
		public int Row;
		public int Found;
		public float[] Height;
	}

	private static volatile Scan pendingScan;

	/// <summary>
	/// Main thread. Builds a new map after landing on a moon (once per moon and day). Only reading the walk floor
	/// from the game happens here; painting it onto the grid and turning it into text (the slow part, which used to
	/// stutter the game for a moment after landing) runs on a background thread.
	/// </summary>
	public static void Tick()
	{
		Scan scan = pendingScan;
		if (scan != null)
		{
			ScanSomeRows(scan);
			return;
		}
		if (building || Time.unscaledTime < nextCheck)
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
		Input input;
		try
		{
			input = Gather(round, rm, key);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Couldn't build the website map: " + ex.Message);
			return;
		}
		building = true;
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Build(input);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Couldn't build the website map: " + ex.Message);
			}
			finally
			{
				building = false;
			}
		});
	}

	/// <summary>
	/// Whether the map for the landing you're on now has been made. It's made a couple of seconds after the ship
	/// lands (and the building is there); until then the website says it's building the map instead of showing the
	/// previous moon's.
	/// </summary>
	public static bool ReadyForCurrent()
	{
		StartOfRound round = StartOfRound.Instance;
		return round != null && round.currentLevel != null && Latest != null && readyFor == round.currentLevel.name + "|" + round.randomMapSeed;
	}

	public static void Reset()
	{
		builtFor = null;
		readyFor = null;
		pendingScan = null;
		Latest = null;
	}

	/// <summary>Hands a finished map to the uploader, unless a newer landing has started since.</summary>
	private static void Publish(string key, JObject map, string note)
	{
		if (key != builtFor)
		{
			return;
		}
		Latest = map.ToString(Formatting.None);
		readyFor = key;
		Debug.Log(note);
	}

	private static Input Gather(StartOfRound round, RoundManager rm, string key)
	{
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		Input input = new Input
		{
			Key = key,
			Moon = round.currentLevel.PlanetName,
			Seed = round.randomMapSeed,
			V = tri.vertices,
			Idx = tri.indices,
			Inside = Nodes(rm.insideAINodes),
			Outside = Nodes(rm.outsideAINodes),
			Ship = round.elevatorTransform != null ? round.elevatorTransform.position : (Vector3?)null,
			Doors = new JArray()
		};
		MineshaftElevatorController lift = rm.currentMineshaftElevator;
		if (lift != null && lift.elevatorTopPoint != null && lift.elevatorBottomPoint != null)
		{
			input.Elevator = new JObject
			{
				["top"] = Point(lift.elevatorTopPoint.position),
				["bottom"] = Point(lift.elevatorBottomPoint.position)
			};
		}
		foreach (EntranceTeleport door in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>())
		{
			if (door == null)
			{
				continue;
			}
			Vector3 p = door.transform.position;
			input.Doors.Add(new JObject
			{
				["x"] = Math.Round(p.x, 1),
				["z"] = Math.Round(p.z, 1),
				["y"] = Math.Round(p.y, 1),
				["inside"] = !door.isEntranceToBuilding,
				["main"] = door.entranceId == 0
			});
		}
		return input;
	}

	/// <summary>Background thread: no game objects in here, only the numbers Gather read.</summary>
	private static void Build(Input input)
	{
		Vector3[] v = input.V;
		int[] idx = input.Idx;
		Vector3[] inside = input.Inside;
		Vector3[] outside = input.Outside;

		// Usually the facility sits far below the moon's surface, so a height split is enough; when it isn't,
		// fall back to the nearest AI node.
		float insideTop = inside.Length > 0 ? inside.Max((Vector3 p) => p.y) : float.MinValue;
		float outsideBottom = outside.Length > 0 ? outside.Min((Vector3 p) => p.y) : float.MaxValue;
		bool splitByHeight = inside.Length > 0 && outside.Length > 0 && outsideBottom - insideTop > 30f;
		float splitY = (insideTop + outsideBottom) / 2f;

		// Where the map can be: around the AI nodes (and the ship, outside). Floor pieces far away from all of
		// them (scenery, bits under the sea) would otherwise stretch the map until the real part is a tiny dot.
		Vector3? shipPos = input.Ship;
		Bounds? outArea = Area(outside, shipPos, 80f);
		Bounds? inArea = Area(inside, null, 30f);

		List<int> inTris = new List<int>();
		List<int> outTris = new List<int>();
		for (int t = 0; t + 2 < idx.Length; t += 3)
		{
			Vector3 c = (v[idx[t]] + v[idx[t + 1]] + v[idx[t + 2]]) / 3f;
			bool nearOut = !outArea.HasValue || InXZ(outArea.Value, c);
			bool nearIn = !inArea.HasValue || InXZ(inArea.Value, c);
			if (!nearOut && !nearIn)
			{
				continue;
			}
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
			if (isInside ? !nearIn : !nearOut)
			{
				continue;
			}
			(isInside ? inTris : outTris).Add(t);
		}

		JObject outsideGrid = Rasterize(v, idx, outTris);
		// Hardly any walk floor outside (the Company has almost none): the ground gets scanned from above instead,
		// back on the main thread (rays can't be cast from here).
		bool scanGround = outTris.Count < 20 && outArea.HasValue;
		// Inside levels with a big drop and no stairs between them (the Mineshaft: the elevator room sits high above
		// the mine) are separate maps; drawn as one, the room covered the tunnels under it. Ordinary buildings have
		// stairs between their floors and stay one map. "inside" is the biggest level (what older websites show).
		List<List<int>> levels = SplitLevels(v, idx, inTris);
		JObject insideGrid;
		JArray levelsJson = null;
		if (levels.Count > 1)
		{
			float[] bounds = BoundsOf(v, idx, inTris);
			levelsJson = new JArray();
			insideGrid = null;
			int biggest = 0;
			foreach (List<int> level in levels.OrderByDescending((List<int> l) => LevelY(v, idx, l, top: true)))
			{
				JObject grid = Rasterize(v, idx, level, bounds);
				levelsJson.Add(new JObject
				{
					["top"] = Math.Round(LevelY(v, idx, level, top: true), 1),
					["bottom"] = Math.Round(LevelY(v, idx, level, top: false), 1),
					["grid"] = grid
				});
				if (level.Count > biggest)
				{
					biggest = level.Count;
					insideGrid = grid;
				}
			}
		}
		else
		{
			insideGrid = Rasterize(v, idx, inTris);
		}
		JObject map = new JObject
		{
			["moon"] = input.Moon,
			["seed"] = input.Seed,
			["outside"] = outsideGrid,
			["inside"] = insideGrid
		};
		if (levelsJson != null)
		{
			map["insideLevels"] = levelsJson;
		}
		if (input.Elevator != null)
		{
			map["elevator"] = input.Elevator;
		}
		map["doors"] = input.Doors;
		string rest = ", inside " + (map["inside"].Type == JTokenType.Null ? "none" : map["inside"]["w"] + "x" + map["inside"]["h"] + (levels.Count > 1 ? " in " + levels.Count + " levels" : ""))
			+ "; " + outTris.Count + "/" + inTris.Count + " floor pieces, " + outside.Length + "/" + inside.Length + " AI nodes";
		string walk = "[LethalMenu] Website map for " + input.Moon + ": outside " + (outsideGrid == null ? "none" : outsideGrid["w"] + "x" + outsideGrid["h"] + " (walk floor)") + rest;
		if (scanGround)
		{
			Bounds area = outArea.Value;
			float cell = Mathf.Max(MinCellSize, Mathf.Max(area.size.x, area.size.z) / MaxCells);
			int w = Mathf.CeilToInt(area.size.x / cell) + 1;
			int h = Mathf.CeilToInt(area.size.z / cell) + 1;
			pendingScan = new Scan { Key = input.Key, Map = map, Note = walk, Rest = rest, Area = area, Cell = cell, W = w, H = h, Height = new float[w * h] };
			return;
		}
		Publish(input.Key, map, walk);
	}

	/// <summary>The box around these points (and the extra one), grown by margin metres. Null with no points.</summary>
	private static Bounds? Area(Vector3[] points, Vector3? extra, float margin)
	{
		if (points.Length == 0 && !extra.HasValue)
		{
			return null;
		}
		Bounds b = new Bounds(points.Length > 0 ? points[0] : extra.Value, Vector3.zero);
		foreach (Vector3 p in points)
		{
			b.Encapsulate(p);
		}
		if (extra.HasValue)
		{
			b.Encapsulate(extra.Value);
		}
		b.Expand(new Vector3(margin * 2f, margin * 2f, margin * 2f));
		return b;
	}

	private static bool InXZ(Bounds b, Vector3 p)
	{
		return p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;
	}

	/// <summary>
	/// Map without walk floor: a ray straight down in every cell. Rays start just above the highest AI node / the
	/// ship, under most roofs, so the floor inside buildings shows too. Only fairly flat ground counts as floor.
	/// About 3000 rays a frame (all of them at once was a hitch), then finished off on a background thread.
	/// </summary>
	private static void ScanSomeRows(Scan scan)
	{
		if (scan.Key != builtFor)
		{
			pendingScan = null;
			return;
		}
		int mask = StartOfRound.Instance != null ? StartOfRound.Instance.collidersAndRoomMaskAndDefault : Physics.DefaultRaycastLayers;
		// area was grown by the margin on y too: its top is about margin metres above the highest point.
		float top = scan.Area.max.y - 80f + 4f;
		float depth = 60f;
		int last = Mathf.Min(scan.H, scan.Row + Mathf.Max(1, 3000 / scan.W));
		for (; scan.Row < last; scan.Row++)
		{
			int cz = scan.Row;
			for (int cx = 0; cx < scan.W; cx++)
			{
				int i = cz * scan.W + cx;
				scan.Height[i] = float.NaN;
				Vector3 from = new Vector3(scan.Area.min.x + (cx + 0.5f) * scan.Cell, top, scan.Area.min.z + (cz + 0.5f) * scan.Cell);
				if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, depth, mask, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.6f)
				{
					scan.Height[i] = hit.point.y;
					scan.Found++;
				}
			}
		}
		if (scan.Row < scan.H)
		{
			return;
		}
		pendingScan = null;
		building = true;
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				string note = scan.Note;
				if (scan.Found >= 20)
				{
					scan.Map["outside"] = Encode(scan.Height, scan.W, scan.H, scan.Area.min.x, scan.Area.min.z, scan.Cell);
					note = "[LethalMenu] Website map for " + scan.Map["moon"] + ": outside " + scan.W + "x" + scan.H + " (ground scan)" + scan.Rest;
				}
				Publish(scan.Key, scan.Map, note);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LethalMenu] Couldn't build the website map: " + ex.Message);
			}
			finally
			{
				building = false;
			}
		});
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
	/// <summary>Height gap with no floor at all in between that makes two separate levels.</summary>
	private const float LevelGap = 12f;

	/// <summary>
	/// Groups floor pieces into levels: sorted by height, a new level starts wherever the next piece is more than
	/// LevelGap higher with nothing in between. Stairs and ramps fill the heights between a building's floors, so
	/// only a real drop (an elevator shaft) splits. Tiny groups (a ledge, a stray piece) join the level next to them.
	/// </summary>
	private static List<List<int>> SplitLevels(Vector3[] v, int[] idx, List<int> tris)
	{
		List<List<int>> levels = new List<List<int>>();
		if (tris.Count == 0)
		{
			return levels;
		}
		List<int> sorted = tris.OrderBy((int t) => (v[idx[t]].y + v[idx[t + 1]].y + v[idx[t + 2]].y) / 3f).ToList();
		List<int> current = new List<int>();
		float last = float.NaN;
		foreach (int t in sorted)
		{
			float y = (v[idx[t]].y + v[idx[t + 1]].y + v[idx[t + 2]].y) / 3f;
			if (!float.IsNaN(last) && y - last > LevelGap)
			{
				levels.Add(current);
				current = new List<int>();
			}
			current.Add(t);
			last = y;
		}
		levels.Add(current);
		for (int i = levels.Count - 1; i >= 0 && levels.Count > 1; i--)
		{
			if (levels[i].Count < 40)
			{
				levels[i > 0 ? i - 1 : i + 1].AddRange(levels[i]);
				levels.RemoveAt(i);
			}
		}
		return levels;
	}

	private static float LevelY(Vector3[] v, int[] idx, List<int> tris, bool top)
	{
		float y = top ? float.MinValue : float.MaxValue;
		foreach (int t in tris)
		{
			for (int k = 0; k < 3; k++)
			{
				y = top ? Mathf.Max(y, v[idx[t + k]].y) : Mathf.Min(y, v[idx[t + k]].y);
			}
		}
		return y;
	}

	/// <summary>minX, minZ, maxX, maxZ of the floor pieces, with a small margin.</summary>
	private static float[] BoundsOf(Vector3[] v, int[] idx, List<int> tris)
	{
		float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
		foreach (int t in tris)
		{
			for (int k = 0; k < 3; k++)
			{
				Vector3 p = v[idx[t + k]];
				minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
				minZ = Mathf.Min(minZ, p.z); maxZ = Mathf.Max(maxZ, p.z);
			}
		}
		return new float[4] { minX - 2f, minZ - 2f, maxX + 2f, maxZ + 2f };
	}

	private static JArray Point(Vector3 p)
	{
		return new JArray(Math.Round(p.x * 2) / 2, Math.Round(p.z * 2) / 2, Math.Round(p.y));
	}

	/// <summary>Floor picture of these pieces. Levels of one building share the same bounds so they line up.</summary>
	private static JObject Rasterize(Vector3[] v, int[] idx, List<int> tris, float[] bounds = null)
	{
		if (tris.Count == 0)
		{
			return null;
		}
		float[] box = bounds ?? BoundsOf(v, idx, tris);
		float minX = box[0], minZ = box[1], maxX = box[2], maxZ = box[3];
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
		return Encode(height, w, h, minX, minZ, cell);
	}

	/// <summary>Floor heights (NaN = no floor) to {x0, z0, cell, w, h, data}.</summary>
	private static JObject Encode(float[] height, int w, int h, float minX, float minZ, float cell)
	{
		float minY = float.MaxValue, maxY = float.MinValue;
		foreach (float y in height)
		{
			if (!float.IsNaN(y))
			{
				minY = Mathf.Min(minY, y);
				maxY = Mathf.Max(maxY, y);
			}
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
