using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using LethalMenu.Cheats;
using LethalMenu.Util;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Handler;

public class EnemyHandler
{
	private EnemyAI enemy;

	private PlayerControllerB target;

	private readonly List<Type> forceDespawnEnemies = new List<Type>
	{
		typeof(ForestGiantAI),
		typeof(SandWormAI),
		typeof(BlobAI),
		typeof(DressGirlAI),
		typeof(PufferAI),
		typeof(SpringManAI),
		typeof(DocileLocustBeesAI),
		typeof(DoublewingAI),
		typeof(RedLocustBees),
		typeof(LassoManAI),
		typeof(JesterAI),
		typeof(RedPillAnomaly),
		typeof(ButlerBeesEnemyAI),
		typeof(RadMechAI),
		typeof(ClaySurgeonAI),
		typeof(CaveDwellerAI),
		typeof(LassoManAI)
	};

	public EnemyHandler(EnemyAI enemy)
	{
		this.enemy = enemy;
	}

	private void HandleLureCrawler()
	{
		CrawlerAI crawlerAI = enemy as CrawlerAI;
		crawlerAI.BeginChasingPlayerServerRpc((int)target.playerClientId);
	}

	private void HandleLureMouthDog()
	{
		MouthDogAI mouthDogAI = enemy as MouthDogAI;
		mouthDogAI.ReactToOtherDogHowl(target.transform.position);
	}

	private void HandleLureBaboonBird()
	{
		BaboonBirdAI baboonBirdAI = enemy as BaboonBirdAI;
		Threat threat = new Threat
		{
			threatScript = target,
			lastSeenPosition = target.transform.position,
			threatLevel = int.MaxValue,
			type = ThreatType.Player,
			focusLevel = int.MaxValue,
			timeLastSeen = Time.time,
			distanceToThreat = 0f,
			distanceMovedTowardsBaboon = float.MaxValue,
			interestLevel = int.MaxValue,
			hasAttacked = true
		};
		baboonBirdAI.SetAggressiveModeServerRpc(1);
		baboonBirdAI.Reflect().Invoke("ReactToThreat", threat);
	}

	private void HandleLureForestGiant()
	{
		ForestGiantAI forestGiantAI = enemy as ForestGiantAI;
		forestGiantAI.SwitchToBehaviourServerRpc(1);
		forestGiantAI.StopSearch(forestGiantAI.roamPlanet, clear: false);
		forestGiantAI.chasingPlayer = target;
		forestGiantAI.investigating = true;
		forestGiantAI.SetDestinationToPosition(target.transform.position);
		forestGiantAI.Reflect().SetValue("lostPlayerInChase", false);
	}

	private void HandleLureCentipede()
	{
		CentipedeAI centipedeAI = enemy as CentipedeAI;
		centipedeAI.SwitchToBehaviourServerRpc(2);
		if (centipedeAI.Reflect().GetValue<bool>("clingingToCeiling"))
		{
			centipedeAI.TriggerCentipedeFallServerRpc(target.playerClientId);
		}
	}

	private void HandleLureFlowerman()
	{
		FlowermanAI flowermanAI = enemy as FlowermanAI;
		flowermanAI.SwitchToBehaviourServerRpc(2);
		flowermanAI.EnterAngerModeServerRpc(20f);
	}

	private void HandleLureSandSpider()
	{
		SandSpiderAI spider = enemy as SandSpiderAI;
		spider.SwitchToBehaviourServerRpc(1);
		int num = spider.SpawnWeb(target.transform.position);
		spider.webTraps.ForEach((SandSpiderWebTrap web) =>
		{
			spider.PlayerTripWebServerRpc(web.trapID, (int)target.playerClientId);
		});
	}

	private void HandleLureRedLocustBees()
	{
		RedLocustBees redLocustBees = enemy as RedLocustBees;
		redLocustBees.SwitchToBehaviourServerRpc(2);
		redLocustBees.hive.isHeld = true;
	}

	private void HandleLureHoarderBug()
	{
		HoarderBugAI hoarderBugAI = enemy as HoarderBugAI;
		hoarderBugAI.SwitchToBehaviourServerRpc(2);
		hoarderBugAI.angryAtPlayer = target;
		hoarderBugAI.angryTimer = float.MaxValue;
		hoarderBugAI.Reflect().SetValue("lostPlayerInChase", false)?.Invoke("SyncNestPositionServerRpc", target.transform.position);
	}

	private void HandleLureNutcrackerEnemy()
	{
		NutcrackerEnemyAI nutcrackerEnemyAI = enemy as NutcrackerEnemyAI;
		nutcrackerEnemyAI.SwitchToBehaviourServerRpc(2);
		nutcrackerEnemyAI.Reflect().SetValue("lastSeenPlayerPos", target.transform.position)?.SetValue("timeSinceSeeingTarget", 0f);
	}

	private void HandleLureMaskedPlayerEnemy()
	{
		MaskedPlayerEnemy maskedPlayerEnemy = enemy as MaskedPlayerEnemy;
		maskedPlayerEnemy.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLureSpringMan()
	{
		SpringManAI springManAI = enemy as SpringManAI;
		springManAI.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLurePuffer()
	{
		PufferAI pufferAI = enemy as PufferAI;
		pufferAI.SwitchToBehaviourServerRpc(2);
	}

	private void HandleLureJester()
	{
		JesterAI jesterAI = enemy as JesterAI;
		jesterAI.SwitchToBehaviourServerRpc(2);
	}

	private void HandleLureSandWorm()
	{
		SandWormAI sandWormAI = enemy as SandWormAI;
		sandWormAI.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLureBushWolf()
	{
		BushWolfEnemy bushWolfEnemy = enemy as BushWolfEnemy;
		bushWolfEnemy.isHiding = false;
		bushWolfEnemy.staringAtPlayer = target; // the player it turns to watch (the old code passed "true", which never fit)
		bushWolfEnemy.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLureRadMech()
	{
		RadMechAI radMechAI = enemy as RadMechAI;
		radMechAI.SetChargingForwardClientRpc(charging: true);
		radMechAI.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLureCaveDweller()
	{
		CaveDwellerAI caveDwellerAI = enemy as CaveDwellerAI;
		caveDwellerAI.SwitchToBehaviourServerRpc(1);
	}

	private void HandleLureEnemyByType()
	{
		EnemyAI enemyAI = enemy;
		EnemyAI enemyAI2 = enemyAI;
		if (!(enemyAI2 is CrawlerAI))
		{
			if (!(enemyAI2 is MouthDogAI))
			{
				if (!(enemyAI2 is BaboonBirdAI))
				{
					if (!(enemyAI2 is ForestGiantAI))
					{
						if (!(enemyAI2 is CentipedeAI))
						{
							if (!(enemyAI2 is FlowermanAI))
							{
								if (!(enemyAI2 is SandSpiderAI))
								{
									if (!(enemyAI2 is RedLocustBees))
									{
										if (!(enemyAI2 is HoarderBugAI))
										{
											if (!(enemyAI2 is NutcrackerEnemyAI))
											{
												if (!(enemyAI2 is MaskedPlayerEnemy))
												{
													if (!(enemyAI2 is SpringManAI))
													{
														if (!(enemyAI2 is PufferAI))
														{
															if (!(enemyAI2 is JesterAI))
															{
																if (!(enemyAI2 is SandWormAI))
																{
																	if (!(enemyAI2 is BushWolfEnemy))
																	{
																		if (!(enemyAI2 is RadMechAI))
																		{
																			if (enemyAI2 is CaveDwellerAI)
																			{
																				HandleLureCaveDweller();
																			}
																			else
																			{
																				enemy.SwitchToBehaviourServerRpc(1);
																			}
																		}
																		else
																		{
																			HandleLureRadMech();
																		}
																	}
																	else
																	{
																		HandleLureBushWolf();
																	}
																}
																else
																{
																	HandleLureSandWorm();
																}
															}
															else
															{
																HandleLureJester();
															}
														}
														else
														{
															HandleLurePuffer();
														}
													}
													else
													{
														HandleLureSpringMan();
													}
												}
												else
												{
													HandleLureMaskedPlayerEnemy();
												}
											}
											else
											{
												HandleLureNutcrackerEnemy();
											}
										}
										else
										{
											HandleLureHoarderBug();
										}
									}
									else
									{
										HandleLureRedLocustBees();
									}
								}
								else
								{
									HandleLureSandSpider();
								}
							}
							else
							{
								HandleLureFlowerman();
							}
						}
						else
						{
							HandleLureCentipede();
						}
					}
					else
					{
						HandleLureForestGiant();
					}
				}
				else
				{
					HandleLureBaboonBird();
				}
			}
			else
			{
				HandleLureMouthDog();
			}
		}
		else
		{
			HandleLureCrawler();
		}
	}

	private void HandleSandWormKillPlayer()
	{
		SandWormAI sandWormAI = enemy as SandWormAI;
		Teleport(target);
		sandWormAI.StartEmergeAnimation();
	}

	private void HandleKillPlayerByType()
	{
		EnemyAI enemyAI = enemy;
		EnemyAI enemyAI2 = enemyAI;
		if (!(enemyAI2 is MouthDogAI mouthDogAI))
		{
			if (!(enemyAI2 is ForestGiantAI forestGiantAI))
			{
				if (!(enemyAI2 is FlowermanAI flowermanAI))
				{
					if (!(enemyAI2 is RedLocustBees redLocustBees))
					{
						if (!(enemyAI2 is NutcrackerEnemyAI nutcrackerEnemyAI))
						{
							if (!(enemyAI2 is MaskedPlayerEnemy maskedPlayerEnemy))
							{
								if (!(enemyAI2 is JesterAI jesterAI))
								{
									if (!(enemyAI2 is SandWormAI))
									{
										if (!(enemyAI2 is CentipedeAI centipedeAI))
										{
											if (!(enemyAI2 is BlobAI blobAI))
											{
												if (!(enemyAI2 is BushWolfEnemy bushWolfEnemy))
												{
													if (enemyAI2 is CaveDwellerAI caveDwellerAI)
													{
														caveDwellerAI.KillPlayerAnimationServerRpc((int)target.playerClientId);
													}
												}
												else
												{
													bushWolfEnemy.DoKillPlayerAnimationServerRpc((int)target.playerClientId);
												}
											}
											else
											{
												blobAI.SlimeKillPlayerEffectServerRpc((int)target.playerClientId);
											}
										}
										else
										{
											centipedeAI.SwitchToBehaviourServerRpc(2);
											centipedeAI.ClingToPlayerServerRpc(target.playerClientId);
										}
									}
									else
									{
										HandleSandWormKillPlayer();
									}
								}
								else
								{
									jesterAI.KillPlayerServerRpc((int)target.playerClientId);
								}
							}
							else
							{
								maskedPlayerEnemy.KillPlayerAnimationServerRpc((int)target.playerClientId);
							}
						}
						else
						{
							nutcrackerEnemyAI.LegKickPlayerServerRpc((int)target.playerClientId);
						}
					}
					else
					{
						redLocustBees.BeeKillPlayerServerRpc((int)target.playerClientId);
					}
				}
				else
				{
					flowermanAI.KillPlayerAnimationServerRpc((int)target.playerClientId);
				}
			}
			else
			{
				forestGiantAI.GrabPlayerServerRpc((int)target.playerClientId);
			}
		}
		else
		{
			mouthDogAI.KillPlayerServerRpc((int)target.playerClientId);
		}
	}

	public bool HasInstaKill()
	{
		List<Type> list = new List<Type>();
		list.Add(typeof(MouthDogAI));
		list.Add(typeof(ForestGiantAI));
		list.Add(typeof(FlowermanAI));
		list.Add(typeof(RedLocustBees));
		list.Add(typeof(NutcrackerEnemyAI));
		list.Add(typeof(MaskedPlayerEnemy));
		list.Add(typeof(JesterAI));
		list.Add(typeof(SandWormAI));
		list.Add(typeof(BlobAI));
		list.Add(typeof(CentipedeAI));
		list.Add(typeof(BushWolfEnemy));
		list.Add(typeof(ClaySurgeonAI));
		list.Add(typeof(RadMechAI));
		list.Add(typeof(CaveDwellerAI));
		List<Type> list2 = list;
		return list2.Contains(enemy.GetType());
	}

	public void Control()
	{
		if (!enemy.isEnemyDead && !(enemy == null))
		{
			global::LethalMenu.Cheats.EnemyControl.Control(enemy);
		}
	}

	public void Kill(bool despawn = false, bool NoAlert = false)
	{
		if (enemy == null || enemy.isEnemyDead || HUDManager.Instance == null || RoundManager.Instance == null)
		{
			return;
		}
		if (PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer))
		{
			if (!enemy.enemyType.canDie)
			{
				enemy.enemyType.canDie = true;
			}
			enemy.KillEnemyServerRpc(forceDespawnEnemies.Contains(enemy.GetType()) | despawn);
		}
		else
		{
			RoundManager.Instance.DespawnEnemyServerRpc(enemy.GetComponent<NetworkObject>());
		}
		if (!NoAlert)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "Killed " + enemy.ShownName());
		}
	}

	public void Stun(bool NoAlert = false)
	{
		if (enemy == null || HUDManager.Instance == null || enemy.isEnemyDead)
		{
			return;
		}
		if (!PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer) && !enemy.enemyType.canBeStunned)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "This enemy can't be stunned without host");
			return;
		}
		if (PlayerHandlerExtensions.IsHost(LethalMenu.localPlayer) && !enemy.enemyType.canBeStunned)
		{
			enemy.enemyType.canBeStunned = true;
		}
		enemy.SetEnemyStunned(setToStunned: true, 5f);
		if (!NoAlert)
		{
			HUDManager.Instance.DisplayTip("Lethal Menu", "Stunned " + enemy.ShownName());
		}
	}

	public void Teleport(PlayerControllerB player = null, Vector3 position = default(Vector3))
	{
		if (!(LethalMenu.localPlayer == null) && !(enemy == null) && !(HUDManager.Instance == null))
		{
			enemy.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
			SyncEnemyPosition(player, position);
		}
	}

	public void TargetPlayer(PlayerControllerB player)
	{
		if (!(LethalMenu.localPlayer == null) && !(enemy == null))
		{
			target = player;
			enemy.targetPlayer = player;
			enemy.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
			enemy.SetMovingTowardsTargetPlayer(player);
			HandleLureEnemyByType();
		}
	}

	public void KillPlayer(PlayerControllerB player)
	{
		if (!(LethalMenu.localPlayer == null) && !(enemy == null))
		{
			target = player;
			enemy.targetPlayer = player;
			enemy.ChangeEnemyOwnerServerRpc(LethalMenu.localPlayer.actualClientId);
			HandleKillPlayerByType();
		}
	}

	public void SyncEnemyPosition(PlayerControllerB player = null, Vector3 position = default(Vector3))
	{
		if (!(enemy == null))
		{
			Vector3 vector = player?.transform.position ?? position;
			if (!(vector == default(Vector3)))
			{
				enemy.transform.position = vector;
				enemy.serverPosition = vector;
				enemy.SyncPositionToClients();
			}
		}
	}

	public static EnemyHandler GetHandler(EnemyAI enemy)
	{
		return new EnemyHandler(enemy);
	}
}
