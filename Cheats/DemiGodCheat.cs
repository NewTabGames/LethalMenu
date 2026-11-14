using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;

namespace LethalMenu.Cheats;

internal class DemiGodCheat : Cheat
{
	public static readonly List<PlayerControllerB> DemiGodPlayers = new List<PlayerControllerB>();

	/// <summary>When each player may be healed again: the new health takes a moment to come back over the
	/// network, and healing every frame until then sent dozens of heals (each with a hit sound).</summary>
	private static readonly Dictionary<PlayerControllerB, float> NextHeal = new Dictionary<PlayerControllerB, float>();

	public override void Update()
	{
		if (Hack.DemiGod.IsEnabled())
		{
			DemiGodPlayers.Where((PlayerControllerB player) => player != null && !player.isPlayerDead && player.health != 100).ToList().ForEach((PlayerControllerB p) =>
			{
				if (NextHeal.TryGetValue(p, out float next) && UnityEngine.Time.time < next)
				{
					return;
				}
				NextHeal[p] = UnityEngine.Time.time + 0.5f;
				p.Handle().Heal();
			});
		}
	}

	public static void ToggleDemiGod(PlayerControllerB player)
	{
		if (!DemiGodPlayers.Contains(player))
		{
			DemiGodPlayers.Add(player);
		}
		else
		{
			DemiGodPlayers.Remove(player);
		}
	}
}
