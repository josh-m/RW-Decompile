using System;

namespace Verse
{
	public sealed class ByteGrid : IExposable
	{
		private byte[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public byte this[IntVec3 c]
		{
			get
			{
				return this.grid[CellIndicesUtility.CellToIndex(c, this.mapSizeX)];
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, this.mapSizeX);
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
				return this.grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)];
			}
			set
			{
				this.grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)] = value;
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
		}

		public ByteGrid(Map map)
		{
			this.ClearAndResizeTo(map);
		}

		public bool MapSizeMatches(Map map)
		{
			return this.mapSizeX == map.Size.x && this.mapSizeZ == map.Size.z;
		}

		public void ClearAndResizeTo(Map map)
		{
			if (this.MapSizeMatches(map) && this.grid != null)
			{
				this.Clear(0);
				return;
			}
			this.mapSizeX = map.Size.x;
			this.mapSizeZ = map.Size.z;
			this.grid = new byte[this.mapSizeX * this.mapSizeZ];
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.mapSizeX, "mapSizeX", 0, false);
			Scribe_Values.LookValue<int>(ref this.mapSizeZ, "mapSizeZ", 0, false);
			ArrayExposeUtility.ExposeByteArray(ref this.grid, "grid");
		}

		public void Clear(byte value = 0)
		{
			if (value == 0)
			{
				Array.Clear(this.grid, 0, this.grid.Length);
			}
			else
			{
				for (int i = 0; i < this.grid.Length; i++)
				{
					this.grid[i] = value;
				}
			}
		}

		public void DebugDraw()
		{
			for (int i = 0; i < this.grid.Length; i++)
			{
				byte b = this.grid[i];
				if (b > 0)
				{
					IntVec3 c = CellIndicesUtility.IndexToCell(i, this.mapSizeX, this.mapSizeZ);
					CellRenderer.RenderCell(c, (float)b / 255f * 0.5f);
				}
			}
		}
	}
}
