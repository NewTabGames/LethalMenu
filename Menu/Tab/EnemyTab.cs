using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalMenu.Handler;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Menu.Tab;

internal class EnemyTab : MenuTab
{
	private readonly string[] tabs = new string[2] { "EnemyTab.EnemyList", "EnemyTab.SpawnEnemies" };

	public static int selectedEnemy = -1;

	public static int selectedEnemyType = -1;

	private int selectedTab = 0;

	private string s_spawnAmount = "1";

	private bool b_spawnOutside = false;

	private Vector2 scrollPos = Vector2.zero;

	private Vector2 scrollPos2 = Vector2.zero;

	private Vector2 scrollPos3 = Vector2.zero;

	public EnemyTab()
		: base("EnemyTab.Title")
	{
	}

	public override void Draw()
	{
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth - (float)HackMenu.Instance.spaceFromLeft));
		selectedTab = GUILayout.Toolbar(selectedTab, Localization.LocalizeArray(tabs));
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.3f - (float)HackMenu.Instance.spaceFromLeft));
		EnemyList();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(HackMenu.Instance.contentWidth * 0.7f - (float)HackMenu.Instance.spaceFromLeft));
		scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
		switch (selectedTab)
		{
		case 0:
			GeneralActions();
			EnemyActions();
			break;
		case 1:
			EnemySpawnerContent();
			break;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private void DrawList<T>(string title, IEnumerable<T> objects, Func<T, bool> conditional, Func<T, string> label, ref Vector2 scroll, ref int instanceID) where T : UnityEngine.Object
	{
		float width = HackMenu.Instance.contentWidth * 0.3f - (float)(HackMenu.Instance.spaceFromLeft * 2);
		float height = HackMenu.Instance.contentHeight - 45f;
		Rect position = new Rect(0f, 30f, width, height);
		GUI.Box(position, Localization.Localize(title));
		GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
		GUILayout.Space(25f);
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		foreach (T @object in objects)
		{
			if (!conditional(@object))
			{
				if (instanceID == -1)
				{
					instanceID = @object.GetInstanceID();
				}
				if (instanceID == @object.GetInstanceID())
				{
					GUI.contentColor = Settings.c_enemyESP.GetColor();
				}
				if (GUILayout.Button(label(@object), GUI.skin.label))
				{
					instanceID = @object.GetInstanceID();
				}
				GUI.contentColor = Settings.c_menuText.GetColor();
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void EnemyList()
	{
		switch (selectedTab)
		{
		case 0:
			if (!LethalMenu.enemies.Exists((EnemyAI e) => e.GetInstanceID() == selectedEnemy))
			{
				selectedEnemy = -1;
			}
			DrawList("EnemyTab.EnemyList", LethalMenu.enemies.OrderBy((EnemyAI e) => EnemyAIExtensions.ShownName(e.enemyType.name)).ToList(), (EnemyAI e) => e.isEnemyDead, (EnemyAI e) => EnemyAIExtensions.ShownName(e.enemyType.name), ref scrollPos, ref selectedEnemy);
			break;
		case 1:
			if (!GameUtil.GetEnemyTypes().Exists((EnemyType e) => e.GetInstanceID() == selectedEnemyType))
			{
				selectedEnemyType = -1;
			}
			DrawList("EnemyTab.EnemyTypes", (from e in GameUtil.GetEnemyTypes()
				orderby EnemyAIExtensions.ShownName(e.name)
				select e).ToList(), (EnemyType _) => false, (EnemyType e) => EnemyAIExtensions.ShownName(e.name), ref scrollPos3, ref selectedEnemyType);
			break;
		}
	}

	private void GeneralActions()
	{
		UI.Header("General.GeneralActions");
		UI.Hack(Hack.EnemyDeathNotifications, "PlayerTab.EnemyDeathNotifications");
		UI.Hack(Hack.KillAllEnemies, "EnemyTab.KillAllEnemies");
		UI.HackSlider(Hack.KillNearbyEnemies, "EnemyTab.KillNearbyEnemies", Settings.f_enemyKillDistance.ToString("0") + "m", ref Settings.f_enemyKillDistance, 5f, 50f, (int)Settings.f_enemyKillDistance);
		UI.Hack(Hack.StunAllEnemies, "EnemyTab.StunAllEnemies");
		// Every monster on the map walks to the player picked in the Players tab (one-shot hunts).
		PlayerControllerB huntTarget = LethalMenu.players.Find((PlayerControllerB p) => p != null && (int)p.playerClientId == PlayersTab.selectedPlayer);
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize("EnemyTab.HuntWithAll") + " (" + (huntTarget == null ? "no player picked" : huntTarget.playerUsername) + ")");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(Localization.Localize("General.Execute")))
		{
			if (huntTarget == null)
			{
				HUDManager.Instance?.DisplayTip("Lethal Menu", "Pick a player in the Players tab first");
			}
			else
			{
				try
				{
					HUDManager.Instance?.DisplayTip("Lethal Menu", Cheats.EnemyHunt.BeginAll(huntTarget));
				}
				catch (Exception ex)
				{
					HUDManager.Instance?.DisplayTip("Lethal Menu", ex.Message);
				}
			}
		}
		GUILayout.EndHorizontal();
		if (LethalMenu.enemies.Exists((EnemyAI e) => e is SandSpiderAI))
		{
			UI.Hack(Hack.BreakAllWebs, "EnemyTab.BreakAllSpiderWeb");
		}
		if (Cheats.EnemyHunt.Hunts.Count > 0)
		{
			UI.Hack(Hack.StopAllHunts, "EnemyTab.StopAllHunts");
		}
		if (Hack.EnemyControl.IsEnabled())
		{
			UI.Button("EnemyTab.StopEnemyControl", () =>
			{
				Hack.EnemyControl.SetToggle(b: false);
			});
			// Same controls the website's "Running now" bar has (T and F9 in game).
			PlayerControllerB chosen = Cheats.EnemyControl.ValidChosenTarget;
			UI.Button(Localization.Localize("EnemyTab.ControlTarget") + ": " + (chosen == null ? "nearest other player" : (Cheats.EnemyControl.TargetingYourself ? "yourself" : chosen.playerUsername)), () =>
			{
				Cheats.EnemyControl.CycleTarget();
			}, "EnemyTab.Change");
			UI.Button(Localization.Localize("EnemyTab.AIDriving") + ": " + (Cheats.EnemyControl.IsAIControlled ? "on" : "off"), () =>
			{
				Cheats.EnemyControl.SetAIDriving(!Cheats.EnemyControl.IsAIControlled);
			}, "EnemyTab.Switch");
		}
	}

	private void EnemyActions()
	{
		UI.Header("EnemyTab.EnemyStatus", space: true);
		if (selectedEnemy == -1)
		{
			UI.Label(Settings.c_error.AsString("EnemyTab.NoEnemy"));
			return;
		}
		PlayerControllerB selectedPlayer = LethalMenu.players.Find((PlayerControllerB p) => (int)p.playerClientId == PlayersTab.selectedPlayer);
		EnemyAI enemy = GetSelectedEnemy();
		string text = ((selectedPlayer == null) ? "None" : selectedPlayer.playerUsername);
		if (Enum.TryParse<global::LethalMenu.Handler.Behaviour>(enemy.currentBehaviourStateIndex.ToString(), out var result))
		{
			UI.Label("EnemyTab.Behaviour", result.ToString());
		}
		UI.Label("EnemyTab.SelectedPlayer", Settings.c_playerESP.AsString(text));
		UI.Label("EnemyTab.Targeting", (enemy.targetPlayer == null) ? "None" : enemy.targetPlayer.playerUsername);
		UI.Label("EnemyTab.Outside", enemy.isOutside.ToString());
		if (enemy is DressGirlAI dressGirlAI)
		{
			UI.Label("EnemyTab.Haunting", (dressGirlAI.hauntingPlayer == null) ? "None" : dressGirlAI.hauntingPlayer.playerUsername);
		}
		UI.Header("EnemyTab.EnemyActions", space: true);
		HoarderBugAI bug = enemy as HoarderBugAI;
		if ((object)bug != null)
		{
			UI.Button("EnemyTab.StealItems", () =>
			{
				bug.StealAllItems();
			});
		}
		SandSpiderAI spider = enemy as SandSpiderAI;
		if ((object)spider != null)
		{
			UI.Button("EnemyTab.SpawnWeb", () =>
			{
				spider.SpawnWeb(spider.abdomen.position);
			});
		}
		UI.Button("EnemyTab.KillEnemy", () =>
		{
			enemy.Handle().Kill();
		});
		UI.Button("EnemyTab.StunEnemy", () =>
		{
			enemy.Handle().Stun();
		});
		UI.Button("EnemyTab.TargetSelectedPlayer", () =>
		{
			enemy.Handle().TargetPlayer(selectedPlayer);
		});
		Cheats.EnemyHunt.Hunt hunt = Cheats.EnemyHunt.Find(enemy);
		if (hunt != null && hunt.Target != null)
		{
			UI.Label("EnemyTab.HuntingNow", hunt.Target.playerUsername);
			UI.Button("EnemyTab.StopHunt", () =>
			{
				Cheats.EnemyHunt.Stop(enemy);
			}, "EnemyTab.Stop");
		}
		UI.Button("EnemyTab.HuntSelectedPlayer", () =>
		{
			string reason = Cheats.EnemyHunt.WhyNot(enemy, selectedPlayer);
			HUDManager.Instance?.DisplayTip("Lethal Menu", reason ?? Cheats.EnemyHunt.Begin(enemy, selectedPlayer));
		});
		if (enemy is MouthDogAI)
		{
			UI.Button("EnemyTab.AggroHuntSelectedPlayer", () =>
			{
				string reason = Cheats.EnemyHunt.WhyNot(enemy, selectedPlayer, persistent: true, aggro: true);
				HUDManager.Instance?.DisplayTip("Lethal Menu", reason ?? Cheats.EnemyHunt.Begin(enemy, selectedPlayer, persistent: true, aggro: true));
			});
		}
		UI.Button("EnemyTab.PersistentHuntSelectedPlayer", () =>
		{
			string reason = Cheats.EnemyHunt.WhyNot(enemy, selectedPlayer, persistent: true);
			HUDManager.Instance?.DisplayTip("Lethal Menu", reason ?? Cheats.EnemyHunt.Begin(enemy, selectedPlayer, persistent: true));
		});
		if (System.Array.IndexOf(Cheats.EnemyHunt.Harmless, enemy.GetType().Name) < 0)
		{
			// Hunts the player down and attacks with its normal damage until they're dead (no instant kill).
			UI.Button("EnemyTab.KillSelectedPlayer", () =>
			{
				string reason = Cheats.EnemyHunt.WhyNot(enemy, selectedPlayer, kill: true);
				HUDManager.Instance?.DisplayTip("Lethal Menu", reason ?? Cheats.EnemyHunt.Begin(enemy, selectedPlayer, kill: true));
			});
		}
		UI.Hack(Hack.TeleportEnemy, "EnemyTab.TeleportSelectedPlayer", selectedPlayer, enemy);
		UI.Hack(Hack.EnemyControl, "EnemyTab.ControlEnemy", enemy);
	}

	private void EnemySpawnerContent()
	{
		if (!StartOfRound.Instance || LethalMenu.localPlayer == null)
		{
			return;
		}
		if (!PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			UI.Label("General.HostRequired", Settings.c_error);
			return;
		}
		if (selectedEnemyType == -1)
		{
			UI.Label("EnemyTab.NoEnemyType", Settings.c_error);
			return;
		}
		EnemyType type = GameUtil.GetEnemyTypes().Find((EnemyType x) => x.GetInstanceID() == selectedEnemyType);
		UI.Label("EnemyTab.SelectedType", EnemyAIExtensions.ShownName(type.name), Settings.c_enemyESP);
		UI.Textbox("EnemyTab.SpawnAmount", ref s_spawnAmount, "[^0-9]");
		if (type.enemyName != "Bush Wolf")
		{
			UI.Checkbox("EnemyTab.SpawnOutside", ref b_spawnOutside);
		}
		UI.Button("EnemyTab.Spawn", () =>
		{
			HackExecutor.SpawnEnemy(type, int.Parse(s_spawnAmount), b_spawnOutside);
		});
	}

	private EnemyAI GetSelectedEnemy()
	{
		return LethalMenu.enemies.Find((EnemyAI x) => x.GetInstanceID() == selectedEnemy);
	}
}
