using LethalMenu.Util;

namespace LethalMenu.Handler.EnemyControl;

/// <summary>
/// Manticoil. Right click takes off / lands; in the air Space flies up and Ctrl flies down.
/// Its own AI (flying off when it sees or hears someone) is paused while you drive it, see DoublewingAIPatches.
/// </summary>
internal class DoublewingController : IEnemyController<DoublewingAI>, IController
{
	private const int Landed = 0;

	private const int Flying = 1;

	public void OnTakeControl(DoublewingAI enemy)
	{
		// Start on the ground; if it was mid-flight it drops down and lands.
		Cheats.EnemyControl.SetFlying(false);
	}

	public void Update(DoublewingAI enemy, bool isAIControlled)
	{
		if (isAIControlled || enemy.isEnemyDead)
		{
			return;
		}
		// Keep the wing animation in step with what you are doing.
		if (Cheats.EnemyControl.IsFlying)
		{
			if (enemy.currentBehaviourStateIndex != Flying)
			{
				enemy.SwitchToBehaviourState(Flying);
			}
		}
		else if (enemy.currentBehaviourStateIndex != Landed && Cheats.EnemyControl.IsOnGround)
		{
			enemy.SwitchToBehaviourState(Landed);
		}
	}

	public void OnReleaseControl(DoublewingAI enemy)
	{
		// An alert that came in while you drove it must not leave the bird ignoring everyone afterwards.
		if (enemy != null)
		{
			enemy.Reflect().SetValue("alertingBird", false);
		}
	}

	public void UsePrimarySkill(DoublewingAI enemy)
	{
		// The screech on its own only plays on your machine. The host can send it to everyone; there is no way
		// for anyone else to ask for that.
		if (enemy.IsServer)
		{
			enemy.BirdScreechClientRpc();
		}
		else
		{
			enemy.Reflect().Invoke("BirdScreech");
		}
	}

	public void UseSecondarySkill(DoublewingAI enemy)
	{
		Cheats.EnemyControl.SetFlying(!Cheats.EnemyControl.IsFlying);
	}

	public string GetPrimarySkillName(DoublewingAI enemy)
	{
		return enemy.IsServer ? "Screech" : "Screech (only you hear it)";
	}

	public string GetSecondarySkillName(DoublewingAI _)
	{
		return Cheats.EnemyControl.IsFlying ? "Land" : "Take off";
	}

	public bool CanUseEntranceDoors(DoublewingAI _)
	{
		return false;
	}

	public float InteractRange(DoublewingAI _)
	{
		return 2.5f;
	}
}
