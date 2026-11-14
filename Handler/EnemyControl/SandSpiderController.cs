using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

internal class SandSpiderController : IEnemyController<SandSpiderAI>, IController
{
	public void UsePrimarySkill(SandSpiderAI enemy)
	{
		PlaceWebTrap(enemy);
	}

	private void PlaceWebTrap(SandSpiderAI enemy)
	{
		if ((bool)StartOfRound.Instance)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			onUnitSphere.y = Mathf.Min(0f, onUnitSphere.y * Random.Range(0.5f, 1f));
			Ray ray = new Ray(enemy.abdomen.position + Vector3.up * 0.4f, onUnitSphere);
			if (Physics.Raycast(ray, out var hitInfo, 7f, StartOfRound.Instance.collidersAndRoomMask) && !(hitInfo.distance < 2f) && Physics.Raycast(enemy.abdomen.position, Vector3.down, out var hitInfo2, 10f, StartOfRound.Instance.collidersAndRoomMask))
			{
				Vector3 startPosition = hitInfo2.point + Vector3.up * 0.2f;
				enemy.SpawnWebTrapServerRpc(startPosition, hitInfo.point);
			}
		}
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
