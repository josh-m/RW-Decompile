using System;

namespace Verse
{
	public class CellIndices
	{
		private int mapSizeX;

		private int mapSizeZ;

		public int NumGridCells
		{
			get
			{
				return this.mapSizeX * this.mapSizeZ;
			}
		}

		public CellIndices(Map map)
		{
			this.mapSizeX = map.Size.x;
			this.mapSizeZ = map.Size.z;
		}

		public int CellToIndex(IntVec3 c)
		{
			return CellIndicesUtility.CellToIndex(c, this.mapSizeX);
		}

		public int CellToIndex(int x, int z)
		{
			return CellIndicesUtility.CellToIndex(x, z, this.mapSizeX);
		}

		public IntVec3 IndexToCell(int ind)
		{
			return CellIndicesUtility.IndexToCell(ind, this.mapSizeX, this.mapSizeZ);
		}
	}
}
