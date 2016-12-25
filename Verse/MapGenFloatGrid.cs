using System;

namespace Verse
{
	public class MapGenFloatGrid
	{
		private Map map;

		private float[] grid;

		public float this[IntVec3 c]
		{
			get
			{
				return this.grid[this.map.cellIndices.CellToIndex(c)];
			}
			set
			{
				this.grid[this.map.cellIndices.CellToIndex(c)] = value;
			}
		}

		public MapGenFloatGrid(string name, Map map)
		{
			this.map = map;
			this.grid = new float[map.cellIndices.NumGridCells];
		}
	}
}
