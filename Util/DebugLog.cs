using System;
using System.Collections.Generic;
using UnityEngine;

namespace LethalMenu.Util;

/// <summary>
/// Keeps the last errors and warnings (the game's and the menu's) so the Debug tab can show them without
/// opening the log file. Repeats of the same line are counted instead of filling the list.
/// </summary>
internal static class DebugLog
{
	internal class Entry
	{
		public string Time;

		public LogType Type;

		public string Message;

		public string Where;

		public int Count = 1;

		public bool FromMenu;
	}

	private const int Keep = 60;

	private static readonly LinkedList<Entry> entries = new LinkedList<Entry>();

	private static readonly object gate = new object();

	private static bool attached;

	internal static bool MenuOnly;

	internal static void Attach()
	{
		if (!attached)
		{
			attached = true;
			Application.logMessageReceivedThreaded += OnLog;
		}
	}

	/// <summary>Called when ejecting, so nothing of the menu stays hooked into the game.</summary>
	internal static void Detach()
	{
		if (attached)
		{
			attached = false;
			Application.logMessageReceivedThreaded -= OnLog;
		}
	}

	internal static void Clear()
	{
		lock (gate)
		{
			entries.Clear();
		}
	}

	internal static List<Entry> Snapshot()
	{
		lock (gate)
		{
			List<Entry> list = new List<Entry>();
			foreach (Entry entry in entries)
			{
				if (!MenuOnly || entry.FromMenu)
				{
					list.Add(new Entry { Time = entry.Time, Type = entry.Type, Message = entry.Message, Where = entry.Where, Count = entry.Count, FromMenu = entry.FromMenu });
				}
			}
			return list;
		}
	}

	private static void OnLog(string message, string stackTrace, LogType type)
	{
		bool fromMenu = (message != null && message.Contains("[LethalMenu]")) || (stackTrace != null && stackTrace.Contains("LethalMenu"));
		if (type == LogType.Log && !fromMenu)
		{
			return; // ordinary game chatter (the Hoarding Bug spam etc.) isn't kept
		}
		string first = (message ?? "").Split('\n')[0];
		if (first.Length > 220)
		{
			first = first.Substring(0, 220) + "...";
		}
		string where = "";
		if (!string.IsNullOrEmpty(stackTrace))
		{
			// The first frame is the place it happened.
			where = stackTrace.Split('\n')[0].Trim();
			if (where.Length > 160)
			{
				where = where.Substring(0, 160) + "...";
			}
		}
		lock (gate)
		{
			Entry last = entries.Last?.Value;
			if (last != null && last.Message == first && last.Type == type)
			{
				last.Count++;
				last.Time = DateTime.Now.ToString("HH:mm:ss");
				return;
			}
			entries.AddLast(new Entry { Time = DateTime.Now.ToString("HH:mm:ss"), Type = type, Message = first, Where = where, FromMenu = fromMenu });
			while (entries.Count > Keep)
			{
				entries.RemoveFirst();
			}
		}
	}
}
