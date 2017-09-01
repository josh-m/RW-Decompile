using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanIncidentUtility
	{
		private const float CaravanPawnsScorePerColonistUndowned = 1f;

		private const float CaravanPawnsScorePerColonistDowned = 0.35f;

		private const float CaravanPawnsScorePerPrisoner = 0.35f;

		private const float CaravanPawnsScorePerAnimalBodySize = 0.2f;

		private const int MapCellsPerCaravanPawnsCountScore = 900;

		private const int MinMapSize = 75;

		private const int MaxMapSize = 110;

		private const float MinEnemyPoints = 45f;

		private static readonly FloatRange EnemyPointsPerCaravanPawnsScoreRange = new FloatRange(10f, 60f);

		public static float CalculateIncidentPoints(IEnumerable<Pawn> playerPawns)
		{
			float num = CaravanIncidentUtility.CalculateCaravanPawnsScore(playerPawns);
			return Mathf.Max(num * CaravanIncidentUtility.EnemyPointsPerCaravanPawnsScoreRange.RandomInRange, 45f);
		}

		public static int CalculateIncidentMapSize(Caravan caravan)
		{
			float num = CaravanIncidentUtility.CalculateCaravanPawnsScore(caravan.PawnsListForReading);
			int num2 = Mathf.RoundToInt(num * 900f);
			return Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float)num2)), 75, 110);
		}

		public static bool CanFireIncidentWhichWantsToGenerateMapAt(int tile)
		{
			if (Current.Game.FindMap(tile) != null)
			{
				return false;
			}
			if (!Find.WorldGrid[tile].biome.implemented)
			{
				return false;
			}
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				if (allWorldObjects[i].Tile == tile && !allWorldObjects[i].def.allowCaravanIncidentsWhichGenerateMap)
				{
					return false;
				}
			}
			return true;
		}

		public static Map SetupCaravanAttackMap(Caravan caravan, List<Pawn> enemies)
		{
			int num = CaravanIncidentUtility.CalculateIncidentMapSize(caravan);
			Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(num, 1, num), WorldObjectDefOf.Ambush);
			IntVec3 playerStartingSpot;
			IntVec3 root;
			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerStartingSpot, out root);
			CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map, 4), CaravanDropInventoryMode.DoNotDrop, true);
			for (int i = 0; i < enemies.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map, 4);
				GenSpawn.Spawn(enemies[i], loc, map, Rot4.Random, false);
			}
			return map;
		}

		public static Map GetOrGenerateMapForIncident(Caravan caravan, IntVec3 size, WorldObjectDef suggestedMapParentDef)
		{
			int tile = caravan.Tile;
			bool flag = Current.Game.FindMap(tile) == null;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, suggestedMapParentDef);
			if (flag && orGenerateMap != null)
			{
				caravan.StoryState.CopyTo(orGenerateMap.StoryState);
			}
			return orGenerateMap;
		}

		private static float CalculateCaravanPawnsScore(IEnumerable<Pawn> caravanPawns)
		{
			float num = 0f;
			foreach (Pawn current in caravanPawns)
			{
				if (current.IsColonist)
				{
					num += (current.Downed ? 0.35f : 1f);
				}
				else if (current.RaceProps.Humanlike)
				{
					num += 0.35f;
				}
				else
				{
					num += 0.2f * current.BodySize;
				}
			}
			return num;
		}
	}
}
