using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_Terrain : GenStep
	{
		private static bool debug_WarnedMissingTerrain;

		public override void Generate(Map map)
		{
			BeachMaker.Init(map);
			RiverMaker river = this.GenerateRiver(map);
			List<IntVec3> list = new List<IntVec3>();
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			MapGenFloatGrid mapGenFloatGrid2 = MapGenerator.FloatGridNamed("Fertility", map);
			TerrainGrid terrainGrid = map.terrainGrid;
			foreach (IntVec3 current in map.AllCells)
			{
				Building edifice = current.GetEdifice(map);
				TerrainDef terrainDef;
				if (edifice != null && edifice.def.Fillage == FillCategory.Full)
				{
					terrainDef = this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], river, true);
				}
				else
				{
					terrainDef = this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], river, false);
				}
				if ((terrainDef == TerrainDefOf.WaterMovingShallow || terrainDef == TerrainDefOf.WaterMovingDeep) && edifice != null)
				{
					list.Add(edifice.Position);
					edifice.Destroy(DestroyMode.Vanish);
				}
				terrainGrid.SetTerrain(current, terrainDef);
			}
			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker current2 in map.Biome.terrainPatchMakers)
			{
				current2.Cleanup();
			}
		}

		private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
		{
			TerrainDef terrainDef = null;
			if (river != null)
			{
				terrainDef = river.TerrainAt(c);
			}
			if (terrainDef == null && preferSolid)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
			}
			TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
			if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
			{
				return terrainDef2;
			}
			if (terrainDef == TerrainDefOf.WaterMovingShallow || terrainDef == TerrainDefOf.WaterMovingDeep)
			{
				return terrainDef;
			}
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			if (terrainDef != null)
			{
				return terrainDef;
			}
			for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
			{
				terrainDef2 = map.Biome.terrainPatchMakers[i].TerrainAt(c, map);
				if (terrainDef2 != null)
				{
					return terrainDef2;
				}
			}
			if (elevation > 0.55f && elevation < 0.61f)
			{
				return TerrainDefOf.Gravel;
			}
			if (elevation >= 0.61f)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
			}
			terrainDef2 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			if (!GenStep_Terrain.debug_WarnedMissingTerrain)
			{
				Log.Error(string.Concat(new object[]
				{
					"No terrain found in biome ",
					map.Biome.defName,
					" for elevation=",
					elevation,
					", fertility=",
					fertility
				}));
				GenStep_Terrain.debug_WarnedMissingTerrain = true;
			}
			return TerrainDefOf.Sand;
		}

		private RiverMaker GenerateRiver(Map map)
		{
			Tile tile = Find.WorldGrid[map.Tile];
			List<Tile.RiverLink> visibleRivers = tile.VisibleRivers;
			if (visibleRivers == null || visibleRivers.Count == 0)
			{
				return null;
			}
			float headingFromTo;
			float angleB;
			if (visibleRivers.Count == 1)
			{
				headingFromTo = Find.WorldGrid.GetHeadingFromTo(map.Tile, visibleRivers[0].neighbor);
				angleB = headingFromTo + 180f;
			}
			else
			{
				List<Tile.RiverLink> list = (from rl in visibleRivers
				orderby -rl.river.degradeThreshold
				select rl).ToList<Tile.RiverLink>();
				headingFromTo = Find.WorldGrid.GetHeadingFromTo(map.Tile, list[0].neighbor);
				angleB = Find.WorldGrid.GetHeadingFromTo(map.Tile, list[1].neighbor);
			}
			Vector3 center = new Vector3(Rand.Range(0.3f, 0.7f) * (float)map.Size.x, 0f, Rand.Range(0.3f, 0.7f) * (float)map.Size.z);
			return new RiverMaker(center, headingFromTo, angleB, (from rl in visibleRivers
			orderby -rl.river.degradeThreshold
			select rl).FirstOrDefault<Tile.RiverLink>().river);
		}
	}
}
