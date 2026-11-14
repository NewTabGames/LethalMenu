using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Cheats;

[HarmonyPatch]
internal class Shoplifter : Cheat
{
	public static Dictionary<string, int> nodeCosts = new Dictionary<string, int>();

	public static int[] itemCosts;

	public static int[] vehicleCosts;

	public override void Update()
	{
		if (LethalMenu.terminal == null)
		{
			return;
		}
		TerminalKeyword terminalKeyword = LethalMenu.terminal.terminalNodes.allKeywords.ToList().Find((TerminalKeyword x) => x.word == "buy");
		if (Hack.Shoplifter.IsEnabled())
		{
			terminalKeyword.compatibleNouns.ToList().ForEach((CompatibleNoun n) =>
			{
				if (!nodeCosts.ContainsKey(n.noun.name))
				{
					nodeCosts.Add(n.noun.name, n.result.itemCost);
				}
				n.result.itemCost = 0;
			});
			if (itemCosts == null)
			{
				itemCosts = new int[LethalMenu.terminal.buyableItemsList.Length];
				for (int num = 0; num < LethalMenu.terminal.buyableItemsList.Length; num++)
				{
					itemCosts[num] = LethalMenu.terminal.buyableItemsList[num].creditsWorth;
				}
			}
			if (vehicleCosts == null)
			{
				vehicleCosts = new int[LethalMenu.terminal.buyableVehicles.Length];
				for (int num2 = 0; num2 < LethalMenu.terminal.buyableVehicles.Length; num2++)
				{
					vehicleCosts[num2] = LethalMenu.terminal.buyableVehicles[num2].creditsWorth;
				}
			}
			LethalMenu.terminal.buyableVehicles.ToList().ForEach((BuyableVehicle i) =>
			{
				i.creditsWorth = 0;
			});
			LethalMenu.terminal.buyableItemsList.ToList().ForEach((Item i) =>
			{
				i.creditsWorth = 0;
			});
		}
		else if (nodeCosts.Count > 0)
		{
			terminalKeyword.compatibleNouns.ToList().FindAll((CompatibleNoun n) => nodeCosts.ContainsKey(n.noun.name)).ForEach((CompatibleNoun n) =>
			{
				n.result.itemCost = nodeCosts[n.noun.name];
			});
			for (int num3 = 0; num3 < LethalMenu.terminal.buyableItemsList.Length; num3++)
			{
				LethalMenu.terminal.buyableItemsList[num3].creditsWorth = itemCosts[num3];
			}
			for (int num4 = 0; num4 < LethalMenu.terminal.buyableVehicles.Length; num4++)
			{
				LethalMenu.terminal.buyableVehicles[num4].creditsWorth = vehicleCosts[num4];
			}
			Clear();
		}
	}

	public static void Clear()
	{
		nodeCosts.Clear();
		itemCosts = null;
		vehicleCosts = null;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
	public static void LoadNode(TerminalNode node)
	{
		if (Hack.Shoplifter.IsEnabled())
		{
			Debug.Log("Making Item Free " + node.name);
			node.itemCost = 0;
		}
	}
}
