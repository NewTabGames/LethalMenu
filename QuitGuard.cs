using System;
using System.Diagnostics;
using UnityEngine;

namespace LethalMenu;

/// <summary>
/// Makes closing the game actually close it. Quitting used to leave Lethal Company hanging on "not responding"
/// until it was ended in Task Manager: the website connection's threads were cut off in the middle of a network
/// read and the game waited on them forever.
///
/// On quit, the website connection is shut down properly first. As a safety net, a small hidden helper then
/// ends the game if it's still running 8 seconds later. A normal quit is done long before that, so the helper
/// finds nothing and exits.
/// </summary>
internal static class QuitGuard
{
	private const int ForceCloseAfterSeconds = 8;

	private static bool installed;

	internal static void Install()
	{
		if (installed)
		{
			return;
		}
		installed = true;
		Application.quitting += OnQuitting;
	}

	private static void OnQuitting()
	{
		try
		{
			Remote.RemoteControl.Shutdown();
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning("[LethalMenu] Closing the website connection: " + ex.Message);
		}
		StartForceClose();
	}

	/// <summary>
	/// Starts "wait 8 seconds, then end this game if it's still there". It runs as its own Windows process because
	/// Unity stops every thread of the game while quitting, so a timer inside the game can't be relied on.
	/// It matches both the process id and the exe name, so it can never close some other program.
	/// </summary>
	private static void StartForceClose()
	{
		try
		{
			Process self = Process.GetCurrentProcess();
			string exe = self.ProcessName + ".exe";
			string command = "/c ping -n " + (ForceCloseAfterSeconds + 1) + " 127.0.0.1 >nul & taskkill /F /FI \"PID eq " + self.Id + "\" /FI \"IMAGENAME eq " + exe + "\" >nul 2>&1";
			Process.Start(new ProcessStartInfo("cmd.exe", command)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			});
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning("[LethalMenu] Couldn't start the quit safety net: " + ex.Message);
		}
	}
}
