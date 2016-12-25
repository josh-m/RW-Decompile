using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Terrain : GenStep
	{
		private static bool debug_WarnedMissingTerrain;

		public override void Generate()
		{
			BeachMaker.Init();
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation");
			MapGenFloatGrid mapGenFloatGrid2 = MapGenerator.FloatGridNamed("Fertility");
			TerrainGrid terrainGrid = Find.TerrainGrid;
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				Building edifice = current.GetEdifice();
				if (edifice != null && edifice.def.Fillage == FillCategory.Full)
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, mapGenFloatGrid[current], mapGenFloatGrid2[current], true));
				}
				else
				{
					terrainGrid.SetTerrain(current, this.TerrainFrom(current, mapGenFloatGrid[current], mapGenFloatGrid2[current], false));
				}
			}
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker current2 in Find.Map.Biome.terrainPatchMakers)
			{
				current2.Cleanup();
			}
		}

		private TerrainDef TerrainFrom(IntVec3 c, float elevation, float fertility, bool requireSolid)
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
			for (int i = 0; i < Find.Map.Biome.terrainPatchMakers.Count; i++)
			{
				terrainDef = Find.Map.Biome.terrainPatchMakers[i].TerrainAt(c);
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
			terrainDef = TerrainThreshold.TerrainAtValue(Find.Map.Biome.terrainsByFertility, fertility);
			if (terrainDef != null)
			{
				return terrainDef;
			}
			if (!GenStep_Terrain.debug_WarnedMissingTerrain)
			{
				Log.Error(string.Concat(new object[]
				{
					"No terrain found in biome ",
					Find.Map.Biome.defName,
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
