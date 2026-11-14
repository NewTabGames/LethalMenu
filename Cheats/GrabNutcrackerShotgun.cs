using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Util;

namespace LethalMenu.Cheats;

[HarmonyPatch]
public class GrabNutcrackerShotgun
{
	[HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
	[HarmonyPrefix]
	public static void BeginGrabObject(PlayerControllerB __instance)
	{
		if (!Hack.GrabNutcrackerShotgun.IsEnabled())
		{
			return;
		}
		GrabbableObject value = __instance.Reflect().GetValue<GrabbableObject>("currentlyGrabbingObject");
		if (value == null)
		{
			return;
		}
		ShotgunItem shotgunItem = value as ShotgunItem;
		if (shotgunItem == null)
		{
			return;
		}
		EnemyAI value2 = shotgunItem.Reflect().GetValue<EnemyAI>("heldByEnemy");
		if (!(value2 == null))
		{
			NutcrackerEnemyAI nutcrackerEnemyAI = value2 as NutcrackerEnemyAI;
			if (!(nutcrackerEnemyAI == null))
			{
				nutcrackerEnemyAI.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
				nutcrackerEnemyAI.DropGunServerRpc(nutcrackerEnemyAI.gunPoint.position);
			}
		}
	}
}
