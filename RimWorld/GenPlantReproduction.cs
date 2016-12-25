using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenPlantReproduction
	{
		public static Plant TryReproduceFrom(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map)
		{
			IntVec3 dest;
			if (!GenPlantReproduction.TryFindReproductionDestination(source, plantDef, mode, map, out dest))
			{
				return null;
			}
			return GenPlantReproduction.TryReproduceInto(dest, plantDef, map);
		}

		public static Plant TryReproduceInto(IntVec3 dest, ThingDef plantDef, Map map)
		{
			if (!plantDef.CanEverPlantAt(dest, map))
			{
				return null;
			}
			if (!GenPlant.SnowAllowsPlanting(dest, map))
			{
				return null;
			}
			return (Plant)GenSpawn.Spawn(plantDef, dest, map);
		}

		public static bool TryFindReproductionDestination(IntVec3 source, ThingDef plantDef, SeedTargFindMode mode, Map map, out IntVec3 foundCell)
		{
			float radius = -1f;
			if (mode == SeedTargFindMode.Reproduce)
			{
				radius = plantDef.plant.reproduceRadius;
			}
			else if (mode == SeedTargFindMode.MapGenCluster)
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
			CellRect cellRect = CellRect.CenteredOn(source, Mathf.RoundToInt(radius));
			cellRect.ClipInsideMap(map);
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c2 = new IntVec3(j, 0, i);
					Plant plant = c2.GetPlant(map);
					if (plant != null)
					{
						num++;
						if (plant.def == plantDef)
						{
							num2++;
						}
					}
					num3 += c2.GetTerrain(map).fertility;
				}
			}
			float num4 = num3 * map.Biome.plantDensity;
			bool flag = (float)num > num4;
			bool flag2 = (float)num > num4 * 1.25f;
			if (flag2)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			BiomeDef curBiome = map.Biome;
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
			Predicate<IntVec3> validator = (IntVec3 c) => plantDef.CanEverPlantAt(c, map) && GenPlant.SnowAllowsPlanting(c, map) && source.InHorDistOf(c, radius) && GenSight.LineOfSight(source, c, map, true);
			return CellFinder.TryFindRandomCellNear(source, map, Mathf.CeilToInt(radius), validator, out foundCell);
		}
	}
}
