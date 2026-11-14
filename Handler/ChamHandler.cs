using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Types;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler;

public class ChamHandler
{
	private static Dictionary<int, Material[]> materials = new Dictionary<int, Material[]>();

	private static Material m_chamMaterial;

	private static int _color;

	public static bool chamsenabled = false;

	private Object @object;

	public ChamHandler(Object obj)
	{
		@object = obj;
	}

	public static void SetupChamMaterial()
	{
		m_chamMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
		{
			hideFlags = (HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor)
		};
		m_chamMaterial.SetInt("_SrcBlend", 5);
		m_chamMaterial.SetInt("_DstBlend", 10);
		m_chamMaterial.SetInt("_Cull", 0);
		m_chamMaterial.SetInt("_ZTest", 8);
		m_chamMaterial.SetInt("_ZWrite", 0);
		m_chamMaterial.SetColor("_Color", Settings.c_chams.GetColor());
		_color = Shader.PropertyToID("_Color");
		LethalMenu.Instance.StartCoroutine(CleanUpMaterials());
	}

	private List<Renderer> GetRenderers()
	{
		List<Renderer> list = new List<Renderer>();
		if (@object == null)
		{
			return list;
		}
		if (@object is GameObject)
		{
			list.AddRange(((GameObject)@object).GetComponentsInChildren<Renderer>());
		}
		if (@object is Component)
		{
			list.AddRange(((Component)@object).GetComponentsInChildren<Renderer>());
		}
		if (@object is DoorLock)
		{
			list.AddRange(((DoorLock)@object).GetComponentsInParent<Renderer>());
		}
		if (@object is RadMechAI)
		{
			list.AddRange(((RadMechAI)@object).GetComponentsInChildren<Renderer>());
		}
		return list;
	}

	public void RemoveCham()
	{
		GetRenderers().ForEach((Renderer r) =>
		{
			if (materials.ContainsKey(r.GetInstanceID()))
			{
				r.SetMaterials(materials[r.GetInstanceID()].ToList());
				materials.Remove(r.GetInstanceID());
			}
		});
	}

	public void ProcessCham(float distance)
	{
		if (!(@object == null))
		{
			bool flag = false;
			if (@object is GrabbableObject { isHeld: false })
			{
				flag = Settings.b_chamsObject;
			}
			if (@object is Landmine)
			{
				flag = Settings.b_chamsLandmine;
			}
			if (@object is PlayerControllerB)
			{
				flag = Settings.b_chamsPlayer;
			}
			if (@object is EnemyAI enemy)
			{
				flag = enemy.GetEnemyAIType().IsESPEnabled() && Settings.b_chamsEnemy;
			}
			if (@object is SteamValveHazard obj && !obj.Reflect().GetValue<bool>("valveHasBeenRepaired"))
			{
				flag = Settings.b_chamsSteamHazard;
			}
			if (@object is TerminalAccessibleObject && ((TerminalAccessibleObject)@object).isBigDoor)
			{
				flag = Settings.b_chamsBigDoor;
			}
			if (@object is DoorLock { isLocked: not false })
			{
				flag = Settings.b_chamsDoorLock;
			}
			if (@object is HangarShipDoor)
			{
				flag = Settings.b_chamsShip;
			}
			if (@object is BreakerBox)
			{
				flag = Settings.b_chamsBreaker;
			}
			if (@object is EnemyVent)
			{
				flag = Settings.b_chamsEnemyVent;
			}
			if (@object is ItemDropship { deliveringOrder: not false })
			{
				flag = Settings.b_chamsItemDropship;
			}
			if (@object is VehicleController)
			{
				flag = Settings.b_chamsCruiser;
			}
			if (@object is GameObject && @object.name.StartsWith("MoldSpore"))
			{
				flag = Settings.b_chamsVainShroud;
			}
			if (@object is MineshaftElevatorController)
			{
				flag = Settings.b_chamsMineshaftElevator;
			}
			if (@object is GameObject && @object.name.StartsWith("AnimContainer"))
			{
				flag = Settings.b_chamsSpikeRoofTrap;
			}
			if (@object is GameObject && @object.name.StartsWith("TurretContainer"))
			{
				flag = Settings.b_chamsTurret;
			}
			if ((chamsenabled & flag) && distance >= Settings.f_chamDistance)
			{
				ApplyCham();
			}
			else
			{
				RemoveCham();
			}
		}
	}

	public void ApplyCham()
	{
		if (@object == null)
		{
			return;
		}
		GetRenderers().ForEach((Renderer r) =>
		{
			if (!(r == null) && !materials.ContainsKey(r.GetInstanceID()) && r.materials != null)
			{
				materials.Add(r.GetInstanceID(), r.materials);
				r.SetMaterials(Enumerable.Repeat(m_chamMaterial, r.materials.Length).ToList());
				UpdateChamColor(r);
			}
		});
	}

	private void UpdateChamColor(Renderer r)
	{
		if (!(r == null) && r.materials != null && !(@object == null))
		{
			Color color = Settings.c_chams.GetColor();
			if (@object is GrabbableObject)
			{
				color = Settings.c_objectChams.GetColor();
			}
			if (@object is Landmine)
			{
				color = Settings.c_landmineChams.GetColor();
			}
			if (@object is EnemyAI)
			{
				color = Settings.c_enemyChams.GetColor();
			}
			if (@object is SteamValveHazard)
			{
				color = Settings.c_steamHazardChams.GetColor();
			}
			if (@object is TerminalAccessibleObject)
			{
				color = Settings.c_bigDoorChams.GetColor();
			}
			if (@object is DoorLock)
			{
				color = Settings.c_doorLockChams.GetColor();
			}
			if (@object is HangarShipDoor)
			{
				color = Settings.c_shipChams.GetColor();
			}
			if (@object is BreakerBox)
			{
				color = Settings.c_breakerChams.GetColor();
			}
			if (@object is EnemyVent)
			{
				color = Settings.c_enemyVentChams.GetColor();
			}
			if (@object is ItemDropship)
			{
				color = Settings.c_itemDropShipChams.GetColor();
			}
			if (@object is VehicleController)
			{
				color = Settings.c_CruiserChams.GetColor();
			}
			if (@object is GameObject && @object.name.StartsWith("MoldSpore"))
			{
				color = Settings.c_vainShroudChams.GetColor();
			}
			if (@object is GameObject && @object.name.StartsWith("AnimContainer"))
			{
				color = Settings.c_spikeRoofTrapChams.GetColor();
			}
			if (@object is GameObject && @object.name.StartsWith("TurretContainer"))
			{
				color = Settings.c_turretChams.GetColor();
			}
			if (@object is MineshaftElevatorController)
			{
				color = Settings.c_mineshaftElevatorChams.GetColor();
			}
			if (Settings.b_UseSingleChamColor)
			{
				color = Settings.c_chams.GetColor();
			}
			r.materials.ToList().ForEach((Material m) =>
			{
				m.SetColor(_color, color);
			});
		}
	}

	public static void RefreshChams()
	{
	}

	public static ChamHandler GetHandler(Object obj)
	{
		return new ChamHandler(obj);
	}

	private static IEnumerator CleanUpMaterials()
	{
		while (true)
		{
			yield return new WaitForSeconds(15f);
			int cnt = 0;
			List<int> keep = new List<int>();
			Object.FindObjectsOfType<Renderer>().ToList().ForEach((Renderer r) =>
			{
				keep.Add(r.GetInstanceID());
			});
			materials.Keys.ToList().FindAll((int k) => !keep.Contains(k)).ForEach((int k) =>
			{
				materials.Remove(k);
				cnt++;
			});
		}
	}

	public static void ChamsSetEnabled(bool enabled)
	{
		chamsenabled = enabled;
	}

	/// <summary>Gives every renderer that still wears a cham material its original materials back (used when ejecting).</summary>
	public static void RestoreAll()
	{
		if (materials.Count > 0)
		{
			foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			{
				if (renderer != null && materials.TryGetValue(renderer.GetInstanceID(), out var original))
				{
					renderer.SetMaterials(original.ToList());
				}
			}
			materials.Clear();
		}
		if (m_chamMaterial != null)
		{
			Object.Destroy(m_chamMaterial);
			m_chamMaterial = null;
		}
	}
}
