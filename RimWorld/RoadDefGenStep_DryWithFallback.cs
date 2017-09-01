using System;
using Verse;

namespace RimWorld
{
	public class RoadDefGenStep_DryWithFallback : RoadDefGenStep
	{
		public TerrainDef fallback;

		public override void Place(Map map, IntVec3 position, TerrainDef rockDef)
		{
			RoadDefGenStep_DryWithFallback.PlaceWorker(map, position, this.fallback);
		}

		public static void PlaceWorker(Map map, IntVec3 position, TerrainDef fallback)
		{
			while (map.terrainGrid.TerrainAt(position).driesTo != null)
			{
				map.terrainGrid.SetTerrain(position, map.terrainGrid.TerrainAt(position).driesTo);
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
			if (terrainDef.passability == Traversability.Impassable || terrainDef == TerrainDefOf.WaterDeep || terrainDef == TerrainDefOf.WaterMovingShallow || terrainDef == TerrainDefOf.WaterMovingDeep)
			{
				map.terrainGrid.SetTerrain(position, fallback);
			}
		}
	}
}
