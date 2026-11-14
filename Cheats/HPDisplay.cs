using TMPro;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class HPDisplay : Cheat
{
	private static TextMeshProUGUI HPText;

	private static GameObject text;

	public override void Update()
	{
		if (!Hack.HPDisplay.IsEnabled() || LethalMenu.localPlayer == null)
		{
			if (HPText != null)
			{
				Object.Destroy(text);
				text = null;
				HPText = null;
			}
			return;
		}
		if (HPText == null)
		{
			GameObject gameObject = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner");
			if (gameObject == null)
			{
				return;
			}
			text = new GameObject("HealthText");
			text.transform.SetParent(gameObject.transform, worldPositionStays: false);
			HPText = text.AddComponent<TextMeshProUGUI>();
			TextMeshProUGUI weightCounter = HUDManager.Instance.weightCounter;
			if (weightCounter.transform.parent == null)
			{
				return;
			}
			RectTransform component = weightCounter.transform.parent.GetComponent<RectTransform>();
			if (component == null)
			{
				return;
			}
			HPText.GetComponent<RectTransform>().localRotation = component.localRotation;
			HPText.font = weightCounter.font;
			HPText.fontSize = weightCounter.fontSize;
			HPText.alignment = TextAlignmentOptions.Center;
			HPText.enableAutoSizing = weightCounter.enableAutoSizing;
			HPText.fontSizeMin = weightCounter.fontSizeMin;
			HPText.fontSizeMax = weightCounter.fontSizeMax;
			HPText.fontSharedMaterial = new Material(weightCounter.fontMaterial);
			RectTransform component2 = text.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0f, 1f);
			component2.anchorMax = new Vector2(0f, 1f);
			component2.pivot = new Vector2(0f, 1f);
			component2.anchoredPosition = new Vector2(-53f, -95f);
			HPText.color = weightCounter.color;
		}
		if (!(HPText == null))
		{
			HPText.text = $"HP \n {LethalMenu.localPlayer.health}";
		}
	}
}
