using System;

namespace Verse
{
	public class MapGenFloatGrid
	{
		private float[] grid;

		public float this[IntVec3 c]
		{
			get
			{
				return this.grid[CellIndices.CellToIndex(c)];
			}
			set
			{
				this.grid[CellIndices.CellToIndex(c)] = value;
			}
		}

		public MapGenFloatGrid(string name)
		{
			this.grid = new float[CellIndices.NumGridCells];
		}
	}
}
