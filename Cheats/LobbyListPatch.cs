using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class LobbyListPatch : Cheat
{
	[HarmonyPatch(typeof(LobbySlot), "Awake")]
	[HarmonyPostfix]
	public static void Awake(LobbySlot __instance)
	{
		__instance.StartCoroutine(CheckLobby(__instance));
	}

	private static IEnumerator CheckLobby(LobbySlot slot)
	{
		yield return new WaitForEndOfFrame();
		if (!string.IsNullOrEmpty(slot.thisLobby.GetData("name")) && AntiKick.HostKickedPlayerList.Contains(slot.thisLobby.Owner.Id) && Settings.b_DisplayHostKickedLobbies)
		{
			ApplyChanges(slot, "Host Kicked", Color.red);
		}
		if (!string.IsNullOrEmpty(slot.thisLobby.GetData("name")) && slot.LobbyName.text.Contains('\u00ad') && Settings.b_DisplayLMUsers)
		{
			ApplyChanges(slot, "Lethal Menu User", Color.magenta);
		}
	}

	private static void ApplyChanges(LobbySlot slot, string text, Color color)
	{
		if (!slot.transform.name.Contains("Challenge"))
		{
			slot.GetComponent<Image>().color = color;
		}
		GameObject gameObject = new GameObject(text);
		gameObject.transform.SetParent(slot.GetComponent<Image>().transform, worldPositionStays: false);
		TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.text = text;
		textMeshProUGUI.font = slot.playerCount.font;
		textMeshProUGUI.fontSize = 15f;
		textMeshProUGUI.alignment = TextAlignmentOptions.Center;
		textMeshProUGUI.color = slot.playerCount.color;
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.raycastTarget = false;
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(400f, 50f);
		component.anchorMin = new Vector2(0.5f, 0f);
		component.anchorMax = new Vector2(0.5f, 0f);
		component.pivot = new Vector2(0.5f, 0f);
		component.anchoredPosition = new Vector2(0f, -10f);
	}
}
