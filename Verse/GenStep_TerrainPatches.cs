using System;
using UnityEngine;

namespace Verse
{
	public class GenStep_TerrainPatches : GenStep
	{
		public TerrainDef terrainDef;

		public FloatRange patchesPer10kCellsRange;

		public FloatRange patchSizeRange;

		public override void Generate(Map map)
		{
			int num = Mathf.RoundToInt((float)map.Area / 10000f * this.patchesPer10kCellsRange.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				float randomInRange = this.patchSizeRange.RandomInRange;
				IntVec3 a = CellFinder.RandomCell(map);
				foreach (IntVec3 current in GenRadial.RadialPatternInRadius(randomInRange / 2f))
				{
					IntVec3 c = a + current;
					if (c.InBounds(map))
					{
						map.terrainGrid.SetTerrain(c, this.terrainDef);
					}
				}
			}
		}
	}
}
