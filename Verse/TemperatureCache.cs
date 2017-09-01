using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class TemperatureCache : IExposable
	{
		private Map map;

		internal TemperatureSaveLoad temperatureSaveLoad;

		public CachedTempInfo[] tempCache;

		private HashSet<int> processedRoomGroupIDs = new HashSet<int>();

		private List<CachedTempInfo> relevantTempInfoList = new List<CachedTempInfo>();

		public TemperatureCache(Map map)
		{
			this.map = map;
			this.tempCache = new CachedTempInfo[map.cellIndices.NumGridCells];
			this.temperatureSaveLoad = new TemperatureSaveLoad(map);
		}

		public void ResetTemperatureCache()
		{
			int numGridCells = this.map.cellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				this.tempCache[i].Reset();
			}
		}

		public void ExposeData()
		{
			this.temperatureSaveLoad.DoExposeWork();
		}

		public void ResetCachedCellInfo(IntVec3 c)
		{
			this.tempCache[this.map.cellIndices.CellToIndex(c)].Reset();
		}

		private void SetCachedCellInfo(IntVec3 c, CachedTempInfo info)
		{
			this.tempCache[this.map.cellIndices.CellToIndex(c)] = info;
		}

		public void TryCacheRegionTempInfo(IntVec3 c, Region reg)
		{
			Room room = reg.Room;
			if (room != null)
			{
				RoomGroup group = room.Group;
				this.SetCachedCellInfo(c, new CachedTempInfo(group.ID, group.CellCount, group.Temperature));
			}
		}

		public bool TryGetAverageCachedRoomGroupTemp(RoomGroup r, out float result)
		{
			CellIndices cellIndices = this.map.cellIndices;
			foreach (IntVec3 current in r.Cells)
			{
				CachedTempInfo item = this.map.temperatureCache.tempCache[cellIndices.CellToIndex(current)];
				if (item.numCells > 0 && !this.processedRoomGroupIDs.Contains(item.roomGroupID))
				{
					this.relevantTempInfoList.Add(item);
					this.processedRoomGroupIDs.Add(item.roomGroupID);
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
			this.processedRoomGroupIDs.Clear();
			this.relevantTempInfoList.Clear();
			return result2;
		}
	}
}
