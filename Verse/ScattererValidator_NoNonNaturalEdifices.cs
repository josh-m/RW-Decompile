using System;

namespace Verse
{
	public class ScattererValidator_NoNonNaturalEdifices : ScattererValidator
	{
		public int radius = 1;

		public override bool Allows(IntVec3 c, Map map)
		{
			CellRect cellRect = CellRect.CenteredOn(c, this.radius);
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c2 = new IntVec3(j, 0, i);
					if (c2.GetEdifice(map) != null)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
