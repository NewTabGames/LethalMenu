using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LethalMenu.Handler;
using UnityEngine;

namespace LethalMenu;

public class Loader : MonoBehaviour
{
	private static GameObject Load;

	/// <summary>True when loaded by the BepInEx plugin rather than an injector.</summary>
	internal static bool RunningUnderBepInEx;

	internal static bool IsLoaded => Load != null;

	/// <summary>Entry point for injectors (SharpMonoInjector: namespace LethalMenu, class Loader, method Init).</summary>
	public static void Init()
	{
		if (Load != null)
		{
			return;
		}
		// BepInEx already provides Harmony (HarmonyX), and a previous injection may have loaded ours: never load a second copy.
		if (!IsAssemblyLoaded("0Harmony"))
		{
			LoadAssembly("LethalMenu.Resources.0Harmony.dll");
		}
		ChamHandler.ChamsSetEnabled(enabled: true);
		Load = new GameObject("LethalMenu");
		Load.AddComponent<LethalMenu>();
		UnityEngine.Object.DontDestroyOnLoad(Load);
	}

	private static bool IsAssemblyLoaded(string name)
	{
		return AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) =>
		{
			try
			{
				return a.GetName().Name == name;
			}
			catch
			{
				return false;
			}
		});
	}

	public static void LoadAssembly(string name)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		using Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name);
		byte[] array = new byte[manifestResourceStream.Length];
		manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
		AppDomain.CurrentDomain.Load(array);
	}

	/// <summary>
	/// Types in this assembly that can actually be loaded. When injected without BepInEx installed,
	/// the BepInEx plugin class cannot be resolved, which would otherwise make GetTypes() throw.
	/// </summary>
	internal static Type[] GetLoadableTypes()
	{
		try
		{
			return Assembly.GetExecutingAssembly().GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type t) => t != null).ToArray();
		}
	}

	/// <summary>Ejects the menu and restores the game (see <see cref="Ejector"/>).</summary>
	public static void Unload()
	{
		Ejector.Eject();
	}

	internal static void DestroyHost()
	{
		if (Load != null)
		{
			UnityEngine.Object.Destroy(Load);
		}
		Load = null;
	}
}
