using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class NoCooldownInteractions : Cheat
{
	private static Dictionary<int, float> triggers = new Dictionary<int, float>();

	/// <summary>Each touched trigger's own interactCooldown value, so ejecting can put it back.</summary>
	private static Dictionary<InteractTrigger, bool> originalCooldowns = new Dictionary<InteractTrigger, bool>();

	/// <summary>Restores hold times and interact cooldowns on every trigger this cheat changed (used when ejecting).</summary>
	public static void RestoreAll()
	{
		LethalMenu.interactTriggers.FindAll((InteractTrigger t) => t != null).ForEach((InteractTrigger t) =>
		{
			if (triggers.TryGetValue(t.GetInstanceID(), out var timeToHold))
			{
				t.timeToHold = timeToHold;
			}
		});
		foreach (KeyValuePair<InteractTrigger, bool> pair in originalCooldowns)
		{
			if (pair.Key != null)
			{
				pair.Key.interactCooldown = pair.Value;
			}
		}
		triggers.Clear();
		originalCooldowns.Clear();
	}

	public NoCooldownInteractions()
	{
		LethalMenu.Instance.StartCoroutine(TimeToHold());
		LethalMenu.Instance.StartCoroutine(CleanUpTriggers());
	}

	public override void Update()
	{
		if (Hack.NoCooldown.IsEnabled())
		{
			LethalMenu.teleporters.FindAll((ShipTeleporter t) => t != null).ForEach((ShipTeleporter t) =>
			{
				t.Reflect().SetValue("cooldownTime", 0f);
				t.buttonTrigger.enabled = true;
			});
		}
	}

	private IEnumerator TimeToHold()
	{
		while (true)
		{
			yield return new WaitForSeconds(2f);
			LethalMenu.interactTriggers.FindAll((InteractTrigger t) => t != null).ForEach((InteractTrigger t) =>
			{
				if (!triggers.ContainsKey(t.GetInstanceID()))
				{
					triggers.Add(t.GetInstanceID(), t.timeToHold);
				}
				float num = (triggers.TryGetValue(t.GetInstanceID(), out var value) ? value : 0.5f);
				if (t.name.Equals("StartGameLever"))
				{
					float num2 = ((TimeOfDay.Instance.daysUntilDeadline > 0) ? (t.timeToHold = 0.7f) : (t.timeToHold = 4f));
					if (num2 != num)
					{
						triggers[t.GetInstanceID()] = num2;
						num = num2;
					}
				}
				float timeToHold = ((!Hack.InstantInteract.IsEnabled()) ? num : (t.name.StartsWith("EntranceTeleport") ? 0.3f : 0f));
				t.timeToHold = timeToHold;
			});
		}
	}

	private IEnumerator CleanUpTriggers()
	{
		while (true)
		{
			yield return new WaitForSeconds(15f);
			List<int> keep = new List<int>();
			LethalMenu.interactTriggers.ForEach((InteractTrigger t) =>
			{
				keep.Add(t.GetInstanceID());
			});
			triggers.Keys.ToList().FindAll((int k) => !keep.Contains(k)).ForEach((int k) =>
			{
				triggers.Remove(k);
			});
			originalCooldowns.Keys.Where((InteractTrigger t) => t == null).ToList().ForEach((InteractTrigger t) =>
			{
				originalCooldowns.Remove(t);
			});
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GrabbableObject), "RequireCooldown")]
	public static bool RequireCD(ref bool __result)
	{
		if (!Hack.NoCooldown.IsEnabled())
		{
			return true;
		}
		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(InteractTrigger), "Interact")]
	public static bool Interact(InteractTrigger __instance)
	{
		if (!originalCooldowns.ContainsKey(__instance))
		{
			originalCooldowns.Add(__instance, __instance.interactCooldown);
		}
		if (__instance.GetComponentInParent<DoorLock>() != null)
		{
			__instance.interactCooldown = true;
			DoorLock componentInParent = __instance.GetComponentInParent<DoorLock>();
			if (componentInParent.Reflect().GetValue<bool>("isDoorOpened"))
			{
				__instance.timeToHold = 0f;
			}
			else
			{
				__instance.timeToHold = (Hack.InstantInteract.IsEnabled() ? 0f : 0.3f);
			}
		}
		else
		{
			__instance.interactCooldown = !Hack.NoCooldown.IsEnabled();
		}
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(StartMatchLever), "BeginHoldingInteractOnLever")]
	public static void BeginHoldPostfix(StartMatchLever __instance)
	{
		if (TimeOfDay.Instance.daysUntilDeadline <= 0)
		{
			float num = ((TimeOfDay.Instance.daysUntilDeadline > 0) ? 0.5f : 4f);
			if (triggers.TryGetValue(__instance.triggerScript.GetInstanceID(), out var value) && value != num)
			{
				triggers[__instance.triggerScript.GetInstanceID()] = num;
			}
			if (Hack.InstantInteract.IsEnabled())
			{
				__instance.triggerScript.timeToHold = 0f;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DoorLock), "UnlockDoor")]
	public static void UnlockDoorPostfix(StartMatchLever __instance)
	{
		if (triggers.TryGetValue(__instance.triggerScript.GetInstanceID(), out var value) && value != 0.3f)
		{
			triggers[__instance.triggerScript.GetInstanceID()] = 0.3f;
		}
		if (Hack.InstantInteract.IsEnabled())
		{
			__instance.triggerScript.timeToHold = 0f;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Shovel), "reelUpShovel")]
	public static IEnumerator reelUpShovelPostfix(IEnumerator reelUpShovel)
	{
		while (reelUpShovel.MoveNext())
		{
			if (!(reelUpShovel.Current is WaitForSeconds) || !Hack.NoCooldown.IsEnabled())
			{
				yield return reelUpShovel.Current;
			}
		}
	}
}
