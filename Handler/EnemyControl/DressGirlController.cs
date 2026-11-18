using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Ghost Girl. Left click chases the player she haunts (the one the game picked). While you drive her, only you
/// see her model (the haunted player's non-owner Update hides her mesh), and touching that haunted player kills
/// them on their own machine.
/// </summary>
internal class DressGirlController : IEnemyController<DressGirlAI>, IController
{
	private float nextChaseStateSync;

	/// <summary>The player the game made her haunt, if you can chase them (not you, still alive).</summary>
	private static PlayerControllerB? ChaseTarget(DressGirlAI enemy)
	{
		PlayerControllerB target = enemy.hauntingPlayer;
		if (target == null || target == LethalMenu.localPlayer || target.isPlayerDead || !target.isPlayerControlled)
		{
			return null;
		}
		return target;
	}

	// Chasing used to retarget her at whoever was closest, but only the player the game picked has the kill
	// check on her (and a remote KillPlayer does nothing), so nobody ever died. She now chases her own target,
	// and her chase state is sent to everyone so the haunted player's game knows she is chasing.
	public void UsePrimarySkill(DressGirlAI enemy)
	{
		if (enemy == null)
		{
			return;
		}
		if (enemy.currentBehaviourStateIndex == 1)
		{
			StopChase(enemy);
			return;
		}
		if (!enemy.IsOwner)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "Taking control of the Ghost Girl, try again");
			return;
		}
		PlayerControllerB? target = ChaseTarget(enemy);
		if (target == null)
		{
			HUDManager.Instance?.DisplayTip("Lethal Menu", "She isn't haunting anyone she can catch right now");
			return;
		}
		((EnemyAI)enemy).EnableEnemyMesh(true, true);
		enemy.Reflect().Invoke("BeginChasing");
		enemy.SwitchToBehaviourServerRpc(1);
		nextChaseStateSync = Time.time + 1f;
		HUDManager.Instance?.DisplayTip("Lethal Menu", "Chasing " + target.playerUsername + " (she is invisible to them while you drive)");
	}

	public void Update(DressGirlAI enemy, bool isAIControlled)
	{
		if (enemy == null || enemy.currentBehaviourStateIndex != 1)
		{
			return;
		}
		// Under F9 the haunted player retakes ownership; their local chaseTimer is 0 so their copy drops to state 0
		// right away. End the chase cleanly instead of leaving a half-chase across F9.
		if (isAIControlled || ChaseTarget(enemy) == null)
		{
			StopChase(enemy);
			return;
		}
		// Keep state 1 synced while you own and drive her so the victim's copy recovers if ownership briefly bounced.
		if (enemy.IsOwner && Time.time >= nextChaseStateSync)
		{
			nextChaseStateSync = Time.time + 1f;
			enemy.SwitchToBehaviourServerRpc(1);
		}
	}

	private static void StopChase(DressGirlAI enemy)
	{
		if (enemy.currentBehaviourStateIndex == 1)
		{
			enemy.Reflect().Invoke("StopChasing");
			enemy.SwitchToBehaviourServerRpc(0);
		}
	}

	public string GetPrimarySkillName(DressGirlAI enemy)
	{
		if (enemy.currentBehaviourStateIndex == 1)
		{
			return "Stop chase";
		}
		PlayerControllerB? target = ChaseTarget(enemy);
		return target == null ? "" : "Chase " + target.playerUsername;
	}

	// Never leave hauntingPlayer empty: once you let go her own Update reads it every frame, and an empty one
	// threw an error each frame so she never haunted anyone again.
	public void OnReleaseControl(DressGirlAI enemy)
	{
		if (!(enemy == null))
		{
			StopChase(enemy);
			enemy.Handle().Teleport(null, new Vector3(-1000f, -1000f));
			((EnemyAI)enemy).EnableEnemyMesh(false, true);
		}
	}

	public void OnDeath(DressGirlAI enemy)
	{
		StopChase(enemy);
		enemy.Handle().Teleport(null, new Vector3(-1000f, -1000f));
		((EnemyAI)enemy).EnableEnemyMesh(false, true);
	}

	public bool CanUseEntranceDoors(DressGirlAI _)
	{
		return true;
	}

	public float InteractRange(DressGirlAI _)
	{
		return 5f;
	}

	public static PlayerControllerB GetClosestPlayer(EnemyAI e)
	{
		// The player picked with T / the website wins while this monster is the one being controlled.
		if (Cheats.EnemyControl.ControlledEnemy == e && Cheats.EnemyControl.ValidChosenTarget != null)
		{
			return Cheats.EnemyControl.ValidChosenTarget;
		}
		return (from p in LethalMenu.players
			where p != null && !p.isPlayerDead
			orderby Vector3.Distance(e.transform.position, p.transform.position)
			select p).FirstOrDefault();
	}
}
