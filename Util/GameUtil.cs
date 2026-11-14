using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Manager;
using UnityEngine;

namespace LethalMenu.Util;

public static class GameUtil
{
	public static List<EnemyType> GetEnemyTypes()
	{
		List<EnemyType> types = new List<EnemyType>();
		if (!StartOfRound.Instance)
		{
			return types;
		}
		if (LethalMenu.quickMenuManager != null)
		{
			types.AddRange(from e in LethalMenu.quickMenuManager.testAllEnemiesLevel.OutsideEnemies
				where e.enemyType.enemyName == "Bush Wolf"
				select e.enemyType);
		}
		SelectableLevel[] levels = StartOfRound.Instance.levels;
		foreach (SelectableLevel selectableLevel in levels)
		{
			selectableLevel.Enemies.ForEach((SpawnableEnemyWithRarity enemy) =>
			{
				if (!types.Contains(enemy.enemyType))
				{
					types.Add(enemy.enemyType);
				}
			});
			selectableLevel.DaytimeEnemies.ForEach((SpawnableEnemyWithRarity enemy) =>
			{
				if (!types.Contains(enemy.enemyType))
				{
					types.Add(enemy.enemyType);
				}
			});
			selectableLevel.OutsideEnemies.ForEach((SpawnableEnemyWithRarity enemy) =>
			{
				if (!types.Contains(enemy.enemyType))
				{
					types.Add(enemy.enemyType);
				}
			});
		}
		return types;
	}

	public static List<SpawnableMapObject> GetSpawnableMapObjects()
	{
		List<SpawnableMapObject> list = new List<SpawnableMapObject>();
		if (StartOfRound.Instance == null || StartOfRound.Instance.levels == null)
		{
			return list;
		}
		SelectableLevel[] levels = StartOfRound.Instance.levels;
		foreach (SelectableLevel selectableLevel in levels)
		{
			if (selectableLevel.spawnableMapObjects == null)
			{
				continue;
			}
			SpawnableMapObject[] spawnableMapObjects = selectableLevel.spawnableMapObjects;
			foreach (SpawnableMapObject spawnableObject in spawnableMapObjects)
			{
				if (!list.Any((SpawnableMapObject x) => x.prefabToSpawn.name == spawnableObject.prefabToSpawn.name))
				{
					list.Add(spawnableObject);
				}
			}
		}
		return list;
	}

	public static void InitializeMapObjects()
	{
		if ((bool)StartOfRound.Instance)
		{
			LethalMenu.turrets.ForEach((Turret t) =>
			{
				t.GetComponent<TerminalAccessibleObject>().InitializeValues();
			});
			LethalMenu.landmines.ForEach((Landmine l) =>
			{
				l.GetComponent<TerminalAccessibleObject>().InitializeValues();
			});
		}
	}

	public static float GetDistanceToPlayer(Vector3 position)
	{
		return Mathf.Round(Vector3.Distance(CameraManager.ActiveCamera.transform.position, position));
	}

	public static float GetDistance(Vector3 from, Vector3 to)
	{
		return Mathf.Round(Vector3.Distance(from, to));
	}

	public static int SphereCastForward(this RaycastHit[] array, Transform transform, float sphereRadius = 1f)
	{
		try
		{
			return Physics.SphereCastNonAlloc(transform.position + transform.forward * (sphereRadius + 1.75f), sphereRadius, transform.forward, array, float.MaxValue);
		}
		catch (NullReferenceException)
		{
			return 0;
		}
	}

	public static RaycastHit[] SphereCastForward(this Transform transform, float sphereRadius = 1f)
	{
		try
		{
			return Physics.SphereCastAll(transform.position + transform.forward * (sphereRadius + 1.75f), sphereRadius, transform.forward, float.MaxValue);
		}
		catch (NullReferenceException)
		{
			return null;
		}
	}

	public static RaycastHit StraightCastForward(this Transform transform, float distance = 1000f)
	{
		Physics.Raycast(transform.position, transform.forward, out var hitInfo, distance);
		return hitInfo;
	}

	public static void RenderPlayerModels()
	{
		PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
		if (Hack.SpectatePlayer.IsEnabled() || Hack.FreeCam.IsEnabled())
		{
			localPlayerController.DisablePlayerModel(localPlayerController.gameObject, enable: true);
			localPlayerController.thisPlayerModelArms.enabled = false;
		}
		else
		{
			localPlayerController.DisablePlayerModel(localPlayerController.gameObject);
			localPlayerController.thisPlayerModelArms.enabled = true;
		}
		PlayerControllerB[] allPlayerScripts = localPlayerController.playersManager.allPlayerScripts;
		foreach (PlayerControllerB playerControllerB in allPlayerScripts)
		{
			if (localPlayerController.playerClientId != playerControllerB.playerClientId)
			{
				if ((SpectatePlayer.isSpectatingPlayer(playerControllerB) || SpectatePlayer.isCamPlayer(playerControllerB)) && Settings.b_disableSpectatorModels)
				{
					playerControllerB.DisablePlayerModel(playerControllerB.gameObject);
					playerControllerB.thisPlayerModelArms.enabled = true;
				}
				else
				{
					playerControllerB.DisablePlayerModel(playerControllerB.gameObject, enable: true, disableLocalArms: true);
					playerControllerB.thisPlayerModelArms.enabled = false;
				}
			}
		}
	}
}
