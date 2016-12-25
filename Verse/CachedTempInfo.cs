using System;

namespace Verse
{
	public struct CachedTempInfo
	{
		public int roomID;

		public int numCells;

		public float temperature;

		public CachedTempInfo(int roomID, int numCells, float temperature)
		{
			this.roomID = roomID;
			this.numCells = numCells;
			this.temperature = temperature;
		}

		public static CachedTempInfo NewCachedTempInfo()
		{
			CachedTempInfo result = default(CachedTempInfo);
			result.Reset();
			return result;
		}

		public void Reset()
		{
			this.roomID = -1;
			this.numCells = 0;
			this.temperature = 0f;
		}
	}
}
