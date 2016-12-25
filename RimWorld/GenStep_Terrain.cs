using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Terrain : GenStep
	{
		private static bool debug_WarnedMissingTerrain;

		public override void Generate(Map map)
		{
			BeachMaker.Init(map);
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation", map);
			MapGenFloatGrid mapGenFloatGrid2 = MapGenerator.FloatGridNamed("Fertility", map);
			TerrainGrid terrainGrid = map.terrainGrid;
			foreach (IntVec3 current in map.AllCells)
			{
				Building edifice = current.GetEdifice(map);
				if (edifice != null && edifice.def.Fillage == FillCategory.Full)
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], true));
				}
				else
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, map, mapGenFloatGrid[current], mapGenFloatGrid2[current], false));
				}
			}
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker current2 in map.Biome.terrainPatchMakers)
			{
				current2.Cleanup();
			}
		}

		private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, bool requireSolid)
		{
			if (requireSolid)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).naturalTerrain;
			}
			TerrainDef terrainDef = BeachMaker.BeachTerrainAt(c);
			if (terrainDef != null)
			{
				return terrainDef;
			}
			for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
			{
				terrainDef = map.Biome.terrainPatchMakers[i].TerrainAt(c, map);
				if (terrainDef != null)
				{
					return terrainDef;
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
			terrainDef = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
			if (terrainDef != null)
			{
				return terrainDef;
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
	}
}
