using System;

namespace Verse
{
	public static class CellIndices
	{
		private static int mapSizeX;

		private static int mapSizeZ;

		public static int NumGridCells
		{
			get
			{
				return CellIndices.mapSizeX * CellIndices.mapSizeZ;
			}
		}

		public static void Reinit()
		{
			CellIndices.mapSizeX = Find.Map.Size.x;
			CellIndices.mapSizeZ = Find.Map.Size.z;
		}

		public static int CellToIndex(IntVec3 c)
		{
			return c.z * CellIndices.mapSizeX + c.x;
		}

		public static int CellToIndex(int x, int z)
		{
			return z * CellIndices.mapSizeX + x;
		}

		public static IntVec3 IndexToCell(int ind)
		{
			int newX = ind % CellIndices.mapSizeX;
			int newZ = ind / CellIndices.mapSizeZ;
			return new IntVec3(newX, 0, newZ);
		}
	}
}
