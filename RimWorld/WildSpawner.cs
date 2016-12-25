using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class WildSpawner
	{
		private const int AnimalCheckInterval = 1210;

		private const float BaseAnimalSpawnChancePerInterval = 0.0268888883f;

		private const int PlantTrySpawnIntervalAt100EdgeLength = 650;

		private static float DesiredAnimalDensity
		{
			get
			{
				float num = Find.Map.Biome.animalDensity;
				float num2 = 0f;
				float num3 = 0f;
				foreach (PawnKindDef current in Find.Map.Biome.AllWildAnimals)
				{
					num3 += current.wildSpawn_EcoSystemWeight;
					if (GenTemperature.SeasonAcceptableFor(current.race))
					{
						num2 += current.wildSpawn_EcoSystemWeight;
					}
				}
				num *= num2 / num3;
				num *= Find.MapConditionManager.AggregateAnimalDensityFactor();
				return num;
			}
		}

		private static float DesiredTotalAnimalWeight
		{
			get
			{
				float desiredAnimalDensity = WildSpawner.DesiredAnimalDensity;
				if (desiredAnimalDensity == 0f)
				{
					return 0f;
				}
				int num = (int)(10000f / desiredAnimalDensity);
				return (float)(Find.Map.Area / num);
			}
		}

		private static float CurrentTotalAnimalWeight
		{
			get
			{
				float num = 0f;
				List<Pawn> allPawnsSpawned = Find.MapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].kindDef.wildSpawn_spawnWild)
					{
						num += allPawnsSpawned[i].kindDef.wildSpawn_EcoSystemWeight;
					}
				}
				return num;
			}
		}

		public static bool AnimalEcosystemFull
		{
			get
			{
				return WildSpawner.CurrentTotalAnimalWeight >= WildSpawner.DesiredTotalAnimalWeight;
			}
		}

		public static void WildSpawnerTick()
		{
			IntVec3 loc;
			if (Find.TickManager.TicksGame % 1210 == 0 && !WildSpawner.AnimalEcosystemFull && Rand.Value < 0.0268888883f * WildSpawner.DesiredAnimalDensity && RCellFinder.TryFindRandomPawnEntryCell(out loc))
			{
				WildSpawner.SpawnRandomWildAnimalAt(loc);
			}
			float num = Find.MapConditionManager.AggregatePlantDensityFactor();
			if (num > 0.0001f)
			{
				int num2 = Find.Map.Size.x * 2 + Find.Map.Size.z * 2;
				int num3 = 650 / (num2 / 100);
				num3 = GenMath.RoundRandom((float)num3 / num);
				if (Find.TickManager.TicksGame % num3 == 0)
				{
					WildSpawner.TrySpawnPlantFromMapEdge();
				}
			}
		}

		private static void TrySpawnPlantFromMapEdge()
		{
			ThingDef plantDef = Find.Map.Biome.AllWildPlants.RandomElementByWeight((ThingDef def) => Find.Map.Biome.CommonalityOfPlant(def));
			IntVec3 cell;
			if (RCellFinder.TryFindRandomCellToPlantInFromOffMap(plantDef, out cell))
			{
				GenPlantReproduction.TrySpawnSeed(cell, plantDef, SeedTargFindMode.MapEdge, null);
			}
		}

		public static void SpawnRandomWildAnimalAt(IntVec3 loc)
		{
			PawnKindDef pawnKindDef = (from a in Find.Map.Biome.AllWildAnimals
			where GenTemperature.SeasonAcceptableFor(a.race)
			select a).RandomElementByWeight((PawnKindDef def) => Find.Map.Biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
			if (pawnKindDef == null)
			{
				Log.Error("No spawnable animals right now.");
				return;
			}
			int randomInRange = pawnKindDef.wildSpawn_GroupSizeRange.RandomInRange;
			int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKindDef.wildSpawn_GroupSizeRange.max));
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, radius);
				Pawn newThing = PawnGenerator.GeneratePawn(pawnKindDef, null);
				GenSpawn.Spawn(newThing, loc2);
			}
		}

		public static string DebugString()
		{
			return string.Concat(new object[]
			{
				"CurrentTotalAnimalWeight: ",
				WildSpawner.CurrentTotalAnimalWeight,
				"\nDesiredAnimalDensity: ",
				WildSpawner.DesiredAnimalDensity,
				"\nDesiredTotalAnimalWeight: ",
				WildSpawner.DesiredTotalAnimalWeight
			});
		}
	}
}
