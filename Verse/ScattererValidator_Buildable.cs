using System;

namespace Verse
{
	public class ScattererValidator_Buildable : ScattererValidator
	{
		public int radius = 1;

		public TerrainAffordance affordance = TerrainAffordance.Heavy;

		public override bool Allows(IntVec3 c, Map map)
		{
			CellRect cellRect = CellRect.CenteredOn(c, this.radius);
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c2 = new IntVec3(j, 0, i);
					if (!c2.InBounds(map))
					{
						return false;
					}
					if (c2.InNoBuildEdgeArea(map))
					{
						return false;
					}
					if (!c2.GetTerrain(map).affordances.Contains(this.affordance))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
