using System;
using System.Collections.Generic;

namespace Verse
{
	public class MapCellsInRandomOrder
	{
		private Map map;

		private List<IntVec3> randomizedCells;

		public MapCellsInRandomOrder(Map map)
		{
			this.map = map;
		}

		public IntVec3 Get(int index)
		{
			if (this.randomizedCells == null)
			{
				this.randomizedCells = new List<IntVec3>(this.map.Area);
				foreach (IntVec3 current in this.map.AllCells)
				{
					this.randomizedCells.Add(current);
				}
				this.randomizedCells.Shuffle<IntVec3>();
			}
			return this.randomizedCells[index];
		}
	}
}
