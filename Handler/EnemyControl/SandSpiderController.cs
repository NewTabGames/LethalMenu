using UnityEngine;
using Control = LethalMenu.Cheats.EnemyControl;

namespace LethalMenu.Handler.EnemyControl;

internal class SandSpiderController : IEnemyController<SandSpiderAI>, IController
{
	public void UsePrimarySkill(SandSpiderAI enemy)
	{
		PlaceWebTrap(enemy);
	}

	private void PlaceWebTrap(SandSpiderAI enemy)
	{
		if (!enemy.IsOwner || !StartOfRound.Instance || enemy.abdomen == null)
		{
			return;
		}
		Control.EnsureMapPropsContainer();
		if (!Physics.Raycast(enemy.abdomen.position, Vector3.down, out var floorHit, 10f, StartOfRound.Instance.collidersAndRoomMask))
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "No floor below for a web");
			return;
		}
		Vector3 origin = enemy.abdomen.position + Vector3.up * 0.4f;
		for (int i = 0; i < 12; i++)
		{
			Vector3 dir = Random.onUnitSphere;
			dir.y = Mathf.Min(0f, dir.y * Random.Range(0.5f, 1f));
			if (Physics.Raycast(new Ray(origin, dir), out var wallHit, 7f, StartOfRound.Instance.collidersAndRoomMask) && wallHit.distance >= 2f)
			{
				Vector3 startPosition = floorHit.point + Vector3.up * 0.2f;
				enemy.SpawnWebTrapServerRpc(startPosition, wallHit.point);
				return;
			}
		}
		HUDManager.Instance?.DisplayTip("Lethal Menu", "No wall close enough for a web");
	}

	public bool CanUseEntranceDoors(SandSpiderAI _)
	{
		return false;
	}

	public string GetPrimarySkillName(SandSpiderAI _)
	{
		return "Place web";
	}
}
