using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class Breadcrumbs : Cheat
{
	private List<Vector3> crumbs = new List<Vector3>();

	private long last = 0L;

	public override void OnGui()
	{
		if (!Hack.Breadcrumbs.IsEnabled())
		{
			return;
		}
		foreach (Vector3 crumb in crumbs)
		{
			if (Cheat.WorldToScreen(crumb, out var screen))
			{
				VisualUtil.DrawString(new Vector2(screen.x, screen.y), crumbs.IndexOf(crumb).ToString(), centered: true, alignMiddle: true);
			}
		}
	}

	public override void Update()
	{
		if ((!StartOfRound.Instance || !StartOfRound.Instance.shipHasLanded) && crumbs.Count > 0)
		{
			crumbs.Clear();
		}
		if (Hack.Breadcrumbs.IsEnabled())
		{
			PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
			long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			if ((float)(num - last) >= Settings.f_breadcrumbInterval * 1000f && !localPlayerController.isPlayerDead)
			{
				last = num;
				Vector3 position = localPlayerController.transform.position;
				position.y -= 0.5f;
				crumbs.Add(position);
			}
		}
	}
}
