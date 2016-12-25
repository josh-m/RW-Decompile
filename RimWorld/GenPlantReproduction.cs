using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenPlantReproduction
	{
		public static bool TrySpawnSeed(IntVec3 cell, ThingDef plantDef, SeedTargFindMode mode, Thing plant = null)
		{
			IntVec3 vec;
			if (!GenPlantReproduction.TryFindSeedTargFor(plantDef, cell, mode, out vec))
			{
				return false;
			}
			Seed seed = (Seed)ThingMaker.MakeThing(plantDef.plant.seedDef, null);
			GenSpawn.Spawn(seed, cell, Rot4.Random);
			seed.Launch(plant, vec, null);
			if (DebugSettings.fastEcology)
			{
				seed.ForceInstantImpact();
			}
			return true;
		}

		public static bool TryFindSeedTargFor(ThingDef plantDef, IntVec3 root, SeedTargFindMode mode, out IntVec3 foundCell)
		{
			float radius = -1f;
			if (mode == SeedTargFindMode.ReproduceSeed)
			{
				radius = plantDef.plant.seedShootRadius;
			}
			else if (mode == SeedTargFindMode.Cluster)
			{
				radius = plantDef.plant.WildClusterRadiusActual;
			}
			else if (mode == SeedTargFindMode.MapEdge)
			{
				radius = 40f;
			}
			int num = 0;
			int num2 = 0;
			float num3 = 0f;
			CellRect cellRect = CellRect.CenteredOn(root, Mathf.RoundToInt(radius));
			cellRect.ClipInsideMap();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c2 = new IntVec3(j, 0, i);
					Plant plant = c2.GetPlant();
					if (plant != null)
					{
						num++;
						if (plant.def == plantDef)
						{
							num2++;
						}
					}
					num3 += c2.GetTerrain().fertility;
				}
			}
			float num4 = num3 * Find.Map.Biome.plantDensity;
			bool flag = (float)num > num4;
			bool flag2 = (float)num > num4 * 1.25f;
			if (flag2)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			BiomeDef curBiome = Find.Map.Biome;
			float num5 = curBiome.AllWildPlants.Sum((ThingDef pd) => curBiome.CommonalityOfPlant(pd));
			float num6 = curBiome.CommonalityOfPlant(plantDef) / num5;
			float num7 = curBiome.CommonalityOfPlant(plantDef) * plantDef.plant.wildCommonalityMaxFraction / num5;
			float num8 = num4 * num7;
			if ((float)num2 > num8)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			float num9 = num4 * num6;
			bool flag3 = (float)num2 < num9 * 0.5f;
			if (flag && !flag3)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			Predicate<IntVec3> validator = (IntVec3 c) => plantDef.CanEverPlantAt(c) && GenPlant.SnowAllowsPlanting(c) && root.InHorDistOf(c, radius) && GenSight.LineOfSight(root, c, true);
			return CellFinder.TryFindRandomCellNear(root, Mathf.CeilToInt(radius), validator, out foundCell);
		}
	}
}
