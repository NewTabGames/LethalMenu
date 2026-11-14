using System;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Handler;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

internal class PlayersTab : MenuTab
{
	public static int selectedPlayer;

	private Vector2 scrollPos = Vector2.zero;

	private Vector2 scrollPos2 = Vector2.zero;

	public PlayersTab()
		: base("PlayerTab.Title")
	{
	}

	public override void Draw()
	{
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.3f - (float)HackMenu.Instance.spaceFromLeft));
		PlayersList();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.7f - (float)HackMenu.Instance.spaceFromLeft));
		scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
		GeneralActions();
		PlayerActions();
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void PlayersList()
	{
		float width = HackMenu.Instance.contentWidth * 0.3f - (float)(HackMenu.Instance.spaceFromLeft * 2);
		float height = HackMenu.Instance.contentHeight - 20f;
		Rect position = new Rect(0f, 0f, width, height);
		GUI.Box(position, Localization.Localize("PlayerTab.PlayerList"));
		GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
		GUILayout.Space(25f);
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		foreach (PlayerControllerB player in LethalMenu.players)
		{
			if (!(player == null) && player.IsRealPlayer())
			{
				if (selectedPlayer == -1)
				{
					selectedPlayer = (int)player.playerClientId;
				}
				if (selectedPlayer == (int)player.playerClientId)
				{
					GUI.contentColor = Settings.c_playerESP.GetColor();
				}
				name = player.playerUsername;
				if (LethalMenu.Instance.LMUsers.ContainsKey(player.playerSteamId.ToString()) && Settings.b_DisplayLMUsers)
				{
					name = "[Lethal Menu " + LethalMenu.Instance.LMUsers[player.playerSteamId.ToString()] + "] " + player.playerUsername;
				}
				if (GUILayout.Button(name, GUI.skin.label))
				{
					selectedPlayer = (int)player.playerClientId;
				}
				GUI.contentColor = Settings.c_menuText.GetColor();
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void GeneralActions()
	{
		UI.Header("General.GeneralActions");
		UI.Hack(Hack.DeathNotifications, "PlayerTab.DeathNotifications");
		UI.Hack(Hack.FreeCam, "PlayerTab.FreeCam");
		if (Hack.SpectatePlayer.IsEnabled())
		{
			UI.Button("PlayerTab.StopSpectating", () =>
			{
				Hack.SpectatePlayer.SetToggle(b: false);
			}, "General.Stop");
		}
		if (Hack.MiniCam.IsEnabled())
		{
			UI.Button("PlayerTab.StopMiniCam", () =>
			{
				Hack.MiniCam.SetToggle(b: false);
			}, "General.Stop");
		}
		UI.Button("PlayerTab.KillEveryone", () =>
		{
			LethalMenu.players.ForEach((PlayerControllerB p) =>
			{
				Hack.KillPlayer.Execute(p);
			});
		});
		UI.Button("PlayerTab.KillEveryoneElse", () =>
		{
			LethalMenu.players.FindAll((PlayerControllerB p) => p.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId).ForEach((PlayerControllerB p) =>
			{
				Hack.KillPlayer.Execute(p);
			});
		});
	}

	private void PlayerActions()
	{
		PlayerControllerB player = LethalMenu.players.Find((PlayerControllerB p) => (int)p.playerClientId == selectedPlayer);
		if (player == null || !player.IsRealPlayer())
		{
			return;
		}
		string text = player.playerUsername;
		if (player.isPlayerDead && player.deadBody != null)
		{
			text = Settings.c_deadPlayer.AsString("PlayerTab.DeadPrefix") + " " + text + " (" + Settings.c_causeOfDeath.AsString(player.deadBody.causeOfDeath.ToString()) + ")";
		}
		JetpackItem jetpackItem = null;
		if (player.currentlyHeldObjectServer is JetpackItem jetpackItem2)
		{
			jetpackItem = jetpackItem2;
		}
		UI.Header(text);
		UI.Header("PlayerTab.PlayerInfo");
		UI.Label("PlayerTab.SteamId", player.playerSteamId.ToString());
		UI.Label("PlayerTab.PlayerId", player.playerClientId.ToString());
		UI.Label("PlayerTab.PlayerStatus", player.isPlayerDead ? "PlayerTab.DeadPrefix" : "PlayerTab.AlivePrefix");
		UI.Label("PlayerTab.PlayerHealth", player.health.ToString());
		UI.Label("PlayerTab.Insanity", player.insanityLevel.ToString());
		UI.Label("PlayerTab.IsHost", PlayerHandlerExtensions.IsHost(player).ToString());
		UI.Label("PlayerTab.IsInFactory", player.isInsideFactory.ToString());
		UI.Label("PlayerTab.IsInShip", player.isInHangarShipRoom.ToString());
		UI.Label("PlayerTab.UsingJetpack", (jetpackItem != null).ToString());
		UI.Header("PlayerTab.Inventory", space: true);
		GrabbableObject[] itemSlots = player.ItemSlots;
		foreach (GrabbableObject grabbableObject in itemSlots)
		{
			if (!(grabbableObject == null))
			{
				UI.Label("", $"Name: {grabbableObject.name} | Value: {grabbableObject.scrapValue} | Weight: {grabbableObject.itemProperties.weight} | Pocketed: {grabbableObject.isPocketed} | Deactivated: {grabbableObject.deactivated}");
			}
		}
		UI.Header("General.GeneralActions", space: true);
		UI.Hack(Hack.Teleport, "PlayerTab.TeleportTo", player.transform.position, player.isInElevator, player.isInHangarShipRoom, player.isInsideFactory);
		if (player != LethalMenu.localPlayer)
		{
			UI.Hack(Hack.DemiGod, "PlayerTab.DemiGod", player);
			Hack.DemiGod.SetToggle(DemiGodCheat.DemiGodPlayers.Contains(player));
		}
		UI.Hack(Hack.KillPlayer, "PlayerTab.Kill", player);
		UI.Hack(Hack.HealPlayer, "PlayerTab.Heal", player);
		UI.Hack(Hack.ExplodeJetpack, "PlayerTab.ExplodeJetpack", jetpackItem);
		UI.Hack(Hack.LightningStrikePlayer, new string[2] { "PlayerTab.Strike", "General.HostStormyTag" }, player);
		UI.Hack(Hack.SpiderWebPlayer, "PlayerTab.SpiderWeb", player);
		UI.Hack(Hack.TeleportAllEnemies, "PlayerTab.TeleportAllEnemies", player, LethalMenu.enemies.ToArray());
		UI.Hack(Hack.LureAllEnemies, "PlayerTab.Lure", player);
		UI.Hack(Hack.ExplodeClosestMine, "PlayerTab.ExplodeMine", player);
		if (player.playerClientId != GameNetworkManager.Instance.localPlayerController.playerClientId)
		{
			string btnText = (SpectatePlayer.isSpectatingPlayer(player) ? "General.Stop" : "PlayerTab.Spectate");
			Action action = () =>
			{
				Hack.SpectatePlayer.SetToggle(b: true);
				Hack.SpectatePlayer.Invoke(player);
			};
			Action action2 = () =>
			{
				Hack.SpectatePlayer.SetToggle(b: false);
			};
			Action action3 = (SpectatePlayer.isSpectatingPlayer(player) ? action2 : action);
			UI.Button("PlayerTab.Spectate", action3, btnText);
			btnText = (((int)player.playerClientId == SpectatePlayer.camPlayer) ? "General.Stop" : "General.View");
			action = () =>
			{
				Hack.MiniCam.SetToggle(b: true);
				Hack.MiniCam.Invoke(player);
			};
			action2 = () =>
			{
				Hack.MiniCam.SetToggle(b: false);
			};
			action3 = (SpectatePlayer.isCamPlayer(player) ? action2 : action);
			UI.Button("PlayerTab.MiniCam", action3, btnText);
		}
	}
}
