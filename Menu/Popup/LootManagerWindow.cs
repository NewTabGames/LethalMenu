using System.Linq;
using LethalMenu.Manager;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Popup;

internal class LootManagerWindow(int id) : PopupMenu("LootManager.Title", new Rect(50f, 50f, 577f, 300f), id)
{
	private string s_search = "";

	private Vector2 scrollPos = Vector2.zero;

	public override void DrawContent(int windowID)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginHorizontal();
		UI.Textbox("General.Search", ref s_search);
		UI.Toggle("LootManager.ShowShipItems", ref Settings.b_ShowShipItems, "General.Enable", "General.Disable");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		UI.ButtonGrid((from i in LethalMenu.items
			where i != null && !i.isHeld && !i.isPocketed
			group i by i.itemProperties.itemName into g
			select g.First()).ToList(), (GrabbableObject i) => $"{i.itemProperties.itemName} {Enumerable.Count(LethalMenu.items, (GrabbableObject ii) => ii.itemProperties.itemName == i.itemProperties.itemName)}x", s_search, TeleportItem, 3);
		GUILayout.EndScrollView();
		GUI.DragWindow();
	}

	private void TeleportItem(GrabbableObject grabbableObject)
	{
		if (!(CameraManager.ActiveCamera == null) && !(HUDManager.Instance == null) && !(StartOfRound.Instance == null))
		{
			Vector3 itemFloorPosition = grabbableObject.GetItemFloorPosition(CameraManager.ActiveCamera.transform.position);
			grabbableObject.targetFloorPosition = ((!StartOfRound.Instance.shipBounds.bounds.Contains(itemFloorPosition)) ? StartOfRound.Instance.propsContainer.InverseTransformPoint(itemFloorPosition) : StartOfRound.Instance.elevatorTransform.InverseTransformPoint(itemFloorPosition));
			HUDManager.Instance.DisplayTip("Lethal Menu", $"Teleported {grabbableObject.itemProperties.itemName} ( {grabbableObject.scrapValue} )!");
		}
	}
}
