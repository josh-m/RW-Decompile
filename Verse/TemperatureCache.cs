using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class TemperatureCache : IExposable
	{
		public CachedTempInfo[] tempCache;

		private HashSet<int> processedRoomIDs = new HashSet<int>();

		private List<CachedTempInfo> relevantTempInfoList = new List<CachedTempInfo>();

		public TemperatureCache()
		{
			GenTemperature.UpdateCachedData();
			this.tempCache = new CachedTempInfo[CellIndices.NumGridCells];
		}

		public void ResetTemperatureCache()
		{
			for (int i = 0; i < CellIndices.NumGridCells; i++)
			{
				this.tempCache[i].Reset();
			}
		}

		public void ExposeData()
		{
			TemperatureSaveLoad.DoExposeWork();
		}

		public void ResetCachedCellInfo(IntVec3 c)
		{
			this.tempCache[CellIndices.CellToIndex(c)].Reset();
		}

		private void SetCachedCellInfo(IntVec3 c, CachedTempInfo info)
		{
			this.tempCache[CellIndices.CellToIndex(c)] = info;
		}

		public void TryCacheRegionTempInfo(IntVec3 c, Region reg)
		{
			Room room = reg.Room;
			if (room != null)
			{
				this.SetCachedCellInfo(c, new CachedTempInfo(room.ID, room.CellCount, room.Temperature));
			}
		}

		public bool TryGetAverageCachedRoomTemp(Room r, out float result)
		{
			foreach (IntVec3 current in r.Cells)
			{
				CachedTempInfo item = Find.Map.temperatureCache.tempCache[CellIndices.CellToIndex(current)];
				if (item.numCells > 0 && !this.processedRoomIDs.Contains(item.roomID))
				{
					this.relevantTempInfoList.Add(item);
					this.processedRoomIDs.Add(item.roomID);
				}
			}
			int num = 0;
			float num2 = 0f;
			foreach (CachedTempInfo current2 in this.relevantTempInfoList)
			{
				num += current2.numCells;
				num2 += current2.temperature * (float)current2.numCells;
			}
			result = num2 / (float)num;
			bool result2 = !this.relevantTempInfoList.NullOrEmpty<CachedTempInfo>();
			this.processedRoomIDs.Clear();
			this.relevantTempInfoList.Clear();
			return result2;
		}
	}
}
