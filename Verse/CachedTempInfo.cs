using System;

namespace Verse
{
	public struct CachedTempInfo
	{
		public int roomGroupID;

		public int numCells;

		public float temperature;

		public CachedTempInfo(int roomGroupID, int numCells, float temperature)
		{
			this.roomGroupID = roomGroupID;
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
			this.roomGroupID = -1;
			this.numCells = 0;
			this.temperature = 0f;
		}
	}
}
