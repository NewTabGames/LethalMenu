using System.Linq;
using LethalMenu.Handler;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Menu.Popup;

internal class ItemManagerWindow(int id) : PopupMenu("ItemManager.Title", new Rect(50f, 50f, 575f, 300f), id)
{
	private Vector2 scrollPos = Vector2.zero;

	private string s_scrapValue = "1000";

	private string s_amount = "1";

	private string s_search = "";

	public override void DrawContent(int windowID)
	{
		if (LethalMenu.localPlayer == null || StartOfRound.Instance == null)
		{
			UI.Label("General.NullError", Settings.c_error);
			GUI.DragWindow();
			return;
		}
		if (!PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			UI.Label("General.HostRequired", Settings.c_error);
			GUI.DragWindow();
			return;
		}
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginHorizontal();
		UI.Textbox("General.Search", ref s_search, "", 0, big: false);
		GUILayout.FlexibleSpace();
		UI.Textbox("ItemManager.ScrapValue", ref s_scrapValue, "[^0-9]", 0, big: false);
		UI.Textbox("ItemManager.Amount", ref s_amount, "[^0-9]", 0, big: false);
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		UI.ButtonGrid(StartOfRound.Instance.allItemsList.itemsList.Where((Item i) => i != null && i.spawnPrefab != null).ToList(), (Item i) => i.name, s_search, (Item i) =>
		{
			SpawnItem(i);
		}, 3);
		GUILayout.EndScrollView();
		GUI.DragWindow();
	}

	private void SpawnItem(Item item)
	{
		if (!(HUDManager.Instance == null))
		{
			int num = (int.TryParse(s_scrapValue, out num) ? num : Random.Range(15, 100));
			int num2 = ((!int.TryParse(s_amount, out num2)) ? 1 : num2);
			for (int i = 0; i < num2; i++)
			{
				GameObject gameObject = Object.Instantiate(item.spawnPrefab, LethalMenu.localPlayer.playerEye.transform.position, Quaternion.identity, StartOfRound.Instance.propsContainer);
				gameObject.GetComponent<GrabbableObject>().SetScrapValue(num);
				gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
				gameObject.GetComponent<NetworkObject>().Spawn();
			}
			HUDManager.Instance.DisplayTip("Lethal Menu", string.Format("Spawned {0} {1}{2} ({3})!", num2, item.itemName, (num2 == 1) ? "" : "s", num));
		}
	}
}
