using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MapCellsInRandomOrder
	{
		private static List<IntVec3> randomizedCells;

		private static IntVec3 lastMapSize;

		public static IntVec3 Get(int index)
		{
			if (MapCellsInRandomOrder.randomizedCells == null || Find.Map.Size != MapCellsInRandomOrder.lastMapSize)
			{
				MapCellsInRandomOrder.randomizedCells = new List<IntVec3>(Find.Map.Area);
				foreach (IntVec3 current in Find.Map.AllCells)
				{
					MapCellsInRandomOrder.randomizedCells.Add(current);
				}
				MapCellsInRandomOrder.randomizedCells.Shuffle<IntVec3>();
				MapCellsInRandomOrder.lastMapSize = Find.Map.Size;
			}
			return MapCellsInRandomOrder.randomizedCells[index];
		}
	}
}
