using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LethalMenu.Cheats;

internal class ItemSlots : Cheat
{
	[HarmonyPatch(typeof(PlayerControllerB), "Awake")]
	public static class AwakePatch
	{
		[HarmonyPatch(typeof(HUDManager), "Awake")]
		public static class HudManagerAwakePatch
		{
			[HarmonyPostfix]
			public static void Postfix(ref Image[] ___itemSlotIcons, ref Image[] ___itemSlotIconFrames)
			{
				if (!Hack.ItemSlots.IsEnabled() || Settings.f_slots == 4f)
				{
					return;
				}
				GameObject gameObject = Object.Instantiate(___itemSlotIconFrames[0].gameObject);
				Transform parent = ___itemSlotIconFrames[0].transform.parent;
				RectTransform component = parent.GetComponent<RectTransform>();
				component.localScale = new Vector3(Mathf.Clamp(1f - (float)((int)Settings.f_slots - 4) * 0.05f, 0.5f, 1f), Mathf.Clamp(1f - (float)((int)Settings.f_slots - 4), 0.5f, 1f), 1f);
				component.anchorMin = new Vector2(0.35f, 0f);
				component.anchorMax = new Vector2(0.65f, 0.3f);
				component.pivot = new Vector2(0.5f, 0f);
				component.anchoredPosition = new Vector2(0f, 0f);
				GridLayoutGroup gridLayoutGroup = parent.gameObject.AddComponent<GridLayoutGroup>();
				gridLayoutGroup.spacing = new Vector2(15f, 15f);
				gridLayoutGroup.cellSize = new Vector2(50f, 50f);
				gridLayoutGroup.childAlignment = TextAnchor.LowerCenter;
				foreach (Component item in parent)
				{
					Object.Destroy(item.gameObject);
				}
				parent.DetachChildren();
				for (int i = 0; i < (int)Settings.f_slots; i++)
				{
					Object.Instantiate(gameObject, parent).name = $"Slot{i}";
				}
				Object.Destroy(gameObject);
				___itemSlotIcons = new Image[(int)Settings.f_slots];
				___itemSlotIconFrames = new Image[(int)Settings.f_slots];
				int num = 0;
				foreach (Transform item2 in parent)
				{
					___itemSlotIcons[num] = item2.GetChild(0).GetComponent<Image>();
					___itemSlotIconFrames[num] = item2.GetComponent<Image>();
					num++;
				}
			}
		}

		[HarmonyPostfix]
		public static void Postfix(PlayerControllerB __instance)
		{
			if (Hack.ItemSlots.IsEnabled() && Settings.f_slots != 4f)
			{
				__instance.ItemSlots = new GrabbableObject[(int)Settings.f_slots];
			}
		}
	}
}
