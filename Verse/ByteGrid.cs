using System;

namespace Verse
{
	public sealed class ByteGrid : IExposable
	{
		private byte[] grid;

		public byte this[IntVec3 c]
		{
			get
			{
				return this.grid[CellIndices.CellToIndex(c)];
			}
			set
			{
				int num = CellIndices.CellToIndex(c);
				this.grid[num] = value;
			}
		}

		public byte this[int index]
		{
			get
			{
				return this.grid[index];
			}
			set
			{
				this.grid[index] = value;
			}
		}

		public byte this[int x, int z]
		{
			get
			{
				return this.grid[CellIndices.CellToIndex(x, z)];
			}
			set
			{
				this.grid[CellIndices.CellToIndex(x, z)] = value;
			}
		}

		public int CellsCount
		{
			get
			{
				return this.grid.Length;
			}
		}

		public ByteGrid()
		{
			if (this.grid == null)
			{
				this.grid = new byte[CellIndices.NumGridCells];
			}
		}

		public void ExposeData()
		{
			ArrayExposeUtility.ExposeByteArray(ref this.grid, "grid");
		}

		public void Clear()
		{
			int numGridCells = CellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				this.grid[i] = 0;
			}
		}

		public void DebugDraw()
		{
			for (int i = 0; i < this.grid.Length; i++)
			{
				byte b = this.grid[i];
				if (b > 0)
				{
					IntVec3 c = CellIndices.IndexToCell(i);
					CellRenderer.RenderCell(c, (float)b / 255f * 0.5f);
				}
			}
		}
	}
}
