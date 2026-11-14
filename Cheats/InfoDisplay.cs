using System;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using LethalMenu.Language;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class InfoDisplay : Cheat
{
	private static string info = "";

	private void Format(string label, params object[] args)
	{
		info = info + string.Format(Localization.Localize(label), args) + "\n";
	}

	public override void OnGui()
	{
		info = "";
		if (Hack.DisplayBodies.IsEnabled())
		{
			Format("Cheats.Info.Bodies", GetBodyCount());
		}
		if (Hack.DisplayEnemies.IsEnabled())
		{
			Format("Cheats.Info.Enemies", GetEnemyCount());
		}
		if (Hack.DisplayMapObjects.IsEnabled())
		{
			Format("Cheats.Info.MapObjects", GetObjectCount(), GetObjectValue());
		}
		if (Hack.DisplayShipObjects.IsEnabled())
		{
			Format("Cheats.Info.ShipObjects", GetShipCount(), GetShipValue());
		}
		if (Hack.DisplayQuota.IsEnabled())
		{
			Format("Cheats.Info.Quota", GetShipValue(), GetQuota());
		}
		if (Hack.DisplayDeadline.IsEnabled())
		{
			Format("Cheats.Info.Deadline", GetDeadline(), $"{GetBuyingRate()}%");
		}
		if (Hack.DisplayCredits.IsEnabled())
		{
			Format("Cheats.Info.Credits", GetCredits());
		}
		Vector2 position = new Vector2(Screen.width - 200, 10f);
		VisualUtil.DrawString(position, info, Settings.c_primary, centered: false, alignMiddle: false, bold: false, forceOnScreen: true, Settings.i_screenFontSize);
	}

	private int GetBodyCount()
	{
		return Enumerable.Count(LethalMenu.players.FindAll((PlayerControllerB player) => player.isPlayerDead));
	}

	private int GetEnemyCount()
	{
		return Enumerable.Count(LethalMenu.enemies.FindAll((EnemyAI enemy) => !enemy.isEnemyDead));
	}

	private int GetShipCount()
	{
		return Enumerable.Count(LethalMenu.items.FindAll((GrabbableObject item) => item.isInShipRoom && !item.isHeld && !item.isPocketed && !item.IsDefaultShipItem()));
	}

	private int GetShipValue()
	{
		return LethalMenu.items.FindAll((GrabbableObject item) => item.isInShipRoom && !item.isHeld && !item.isPocketed && !item.IsDefaultShipItem()).Sum((GrabbableObject item) => item.scrapValue);
	}

	private int GetObjectValue()
	{
		return LethalMenu.items.FindAll((GrabbableObject item) => !item.isInShipRoom && !item.isHeld && !item.isPocketed && !item.IsDefaultShipItem()).Sum((GrabbableObject item) => item.scrapValue);
	}

	private int GetObjectCount()
	{
		return Enumerable.Count(LethalMenu.items.FindAll((GrabbableObject item) => !item.isInShipRoom && !item.isHeld && !item.isPocketed && !item.IsDefaultShipItem()));
	}

	private int GetQuota()
	{
		if (!StartOfRound.Instance)
		{
			return 0;
		}
		return TimeOfDay.Instance.profitQuota;
	}

	private int GetDeadline()
	{
		if (!StartOfRound.Instance)
		{
			return 0;
		}
		return TimeOfDay.Instance.daysUntilDeadline;
	}

	private int GetCredits()
	{
		if (!LethalMenu.terminal)
		{
			return 0;
		}
		return LethalMenu.terminal.groupCredits;
	}

	private float GetBuyingRate()
	{
		if (!StartOfRound.Instance)
		{
			return 0f;
		}
		return (float)Math.Round(StartOfRound.Instance.companyBuyingRate * 100f, 2);
	}
}
