using System;
using System.Collections.Generic;

namespace LethalMenu.Types;

public static class EnemyAITypeExtensions
{
	public static readonly Dictionary<EnemyAIType, bool> EnemyFilter = new Dictionary<EnemyAIType, bool>
	{
		{
			EnemyAIType.Unknown,
			true
		},
		{
			EnemyAIType.Centipede,
			true
		},
		{
			EnemyAIType.SandSpider,
			true
		},
		{
			EnemyAIType.HoarderBug,
			true
		},
		{
			EnemyAIType.Flowerman,
			true
		},
		{
			EnemyAIType.Crawler,
			true
		},
		{
			EnemyAIType.Blob,
			true
		},
		{
			EnemyAIType.DressGirl,
			true
		},
		{
			EnemyAIType.Puffer,
			true
		},
		{
			EnemyAIType.Nutcracker,
			true
		},
		{
			EnemyAIType.RedLocustBees,
			true
		},
		{
			EnemyAIType.Doublewing,
			false
		},
		{
			EnemyAIType.DocileLocustBees,
			false
		},
		{
			EnemyAIType.MouthDog,
			true
		},
		{
			EnemyAIType.ForestGiant,
			true
		},
		{
			EnemyAIType.SandWorm,
			true
		},
		{
			EnemyAIType.BaboonHawk,
			true
		},
		{
			EnemyAIType.SpringMan,
			true
		},
		{
			EnemyAIType.Jester,
			true
		},
		{
			EnemyAIType.LassoMan,
			true
		},
		{
			EnemyAIType.MaskedPlayerEnemy,
			true
		},
		{
			EnemyAIType.Butler,
			true
		},
		{
			EnemyAIType.ButlerBees,
			true
		},
		{
			EnemyAIType.RadMech,
			true
		},
		{
			EnemyAIType.FlowerSnake,
			true
		},
		{
			EnemyAIType.BushWolf,
			true
		},
		{
			EnemyAIType.ClaySurgeon,
			true
		},
		{
			EnemyAIType.CaveDweller,
			true
		}
	};

	public static void ToggleESP(this EnemyAIType type)
	{
		EnemyFilter[type] = !EnemyFilter[type];
	}

	public static bool IsESPEnabled(this EnemyAIType type)
	{
		return EnemyFilter[type];
	}

	public static EnemyAIType GetEnemyAIType(this EnemyAI enemy)
	{
		if (Enum.TryParse<EnemyAIType>(enemy.enemyType.name, out var result))
		{
			return result;
		}
		return EnemyAIType.Unknown;
	}
}
