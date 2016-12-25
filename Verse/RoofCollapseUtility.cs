using System;

namespace Verse
{
	public static class RoofCollapseUtility
	{
		public const float RoofMaxSupportDistance = 6.9f;

		public static readonly int RoofSupportRadialCellsCount = GenRadial.NumCellsInRadius(6.9f);

		public static bool WithinRangeOfRoofHolder(IntVec3 c)
		{
			Building[] innerArray = Find.EdificeGrid.InnerArray;
			for (int i = 0; i < RoofCollapseUtility.RoofSupportRadialCellsCount; i++)
			{
				IntVec3 c2 = c + GenRadial.RadialPattern[i];
				if (c2.InBounds())
				{
					Thing thing = innerArray[CellIndices.CellToIndex(c2)];
					if (thing != null && thing.def.holdsRoof)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool ConnectedToRoofHolder(IntVec3 c, bool assumeRoofAtRoot)
		{
			bool connected = false;
			FloodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed() || (x == c && assumeRoofAtRoot)) && !connected, delegate(IntVec3 x)
			{
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
					if (c2.InBounds())
					{
						Building edifice = c2.GetEdifice();
						if (edifice != null && edifice.def.holdsRoof)
						{
							connected = true;
							break;
						}
					}
				}
			});
			return connected;
		}
	}
}
