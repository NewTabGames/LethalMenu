using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class LootThroughWallsBeltBag
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(BeltBagItem), "ItemInteractLeftRight")]
	public static bool ItemInteractLeftRight(BeltBagItem __instance, bool right)
	{
		if ((__instance.playerHeldBy == null || __instance.tryingAddToBag || __instance.objectsInBag.Count >= 15) | right)
		{
			return true;
		}
		float maxDistance = (Hack.LootThroughWallsBeltBag.IsEnabled() ? float.MaxValue : 4f);
		LayerMask layerMask = (Hack.LootThroughWallsBeltBag.IsEnabled() ? ((LayerMask)LayerMask.GetMask("Props")) : ((LayerMask)1073742144));
		if (Physics.Raycast(__instance.playerHeldBy.gameplayCamera.transform.position, __instance.playerHeldBy.gameplayCamera.transform.forward, out var hitInfo, maxDistance, layerMask.value, QueryTriggerInteraction.Ignore))
		{
			GrabbableObject component = hitInfo.collider.gameObject.GetComponent<GrabbableObject>();
			if (component != null && !component.isHeld && !component.isHeldByEnemy && (Hack.LootAnyItemBeltBag.IsEnabled() || (!component.itemProperties.isScrap && component.itemProperties.itemId != 123984 && component.itemProperties.itemId != 819501)))
			{
				if (Hack.LootAnyItemBeltBag.IsEnabled() && component is LungProp { isLungDocked: not false } lungProp)
				{
					lungProp.EquipItem();
				}
				__instance.TryAddObjectToBag(component);
			}
		}
		return false;
	}
}
