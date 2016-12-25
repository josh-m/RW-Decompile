using System;
using System.Collections.Generic;

namespace Verse
{
	public class RegionAndRoomUpdater
	{
		private Map map;

		private List<Region> newRegions = new List<Region>();

		private HashSet<Room> reusedOldRooms = new HashSet<Room>();

		private List<Room> newRooms = new List<Room>();

		private List<Region> regionGroup = new List<Region>();

		private bool initialized;

		private bool working;

		private bool enabledInt = true;

		public bool Enabled
		{
			get
			{
				return this.enabledInt;
			}
			set
			{
				this.enabledInt = value;
			}
		}

		public RegionAndRoomUpdater(Map map)
		{
			this.map = map;
		}

		public void RebuildAllRegionsAndRooms()
		{
			this.map.temperatureCache.ResetTemperatureCache();
			this.map.regionDirtyer.SetAllDirty();
			this.RebuildDirtyRegionsAndRooms();
		}

		public void RebuildDirtyRegionsAndRooms()
		{
			if (this.working || !this.Enabled)
			{
				return;
			}
			this.working = true;
			if (!this.initialized)
			{
				this.RebuildAllRegionsAndRooms();
			}
			if (!this.map.regionDirtyer.AnyDirty)
			{
				this.working = false;
				return;
			}
			ProfilerThreadCheck.BeginSample("RebuildDirtyRegionsAndRooms");
			ProfilerThreadCheck.BeginSample("Regenerate new regions from dirty cells");
			for (int i = 0; i < this.map.regionDirtyer.DirtyCells.Count; i++)
			{
				IntVec3 intVec = this.map.regionDirtyer.DirtyCells[i];
				if (this.map.regionGrid.GetValidRegionAt(intVec) == null)
				{
					Region region = this.map.regionMaker.TryGenerateRegionFrom(intVec);
					if (region != null)
					{
						this.newRegions.Add(region);
					}
				}
			}
			ProfilerThreadCheck.EndSample();
			ProfilerThreadCheck.BeginSample("Combine new regions into contiguous groups");
			int num = 0;
			for (int j = 0; j < this.newRegions.Count; j++)
			{
				if (this.newRegions[j].newRegionGroupIndex < 0)
				{
					RegionTraverser.FloodAndSetNewRegionIndex(this.newRegions[j], num);
					num++;
				}
			}
			ProfilerThreadCheck.EndSample();
			ProfilerThreadCheck.BeginSample("Process " + num + " new region groups");
			for (int k = 0; k < num; k++)
			{
				ProfilerThreadCheck.BeginSample("Remake regionGroup list");
				this.regionGroup.Clear();
				for (int l = 0; l < this.newRegions.Count; l++)
				{
					if (this.newRegions[l].newRegionGroupIndex == k)
					{
						this.regionGroup.Add(this.newRegions[l]);
					}
				}
				ProfilerThreadCheck.EndSample();
				if (this.regionGroup.Count == 1 && this.regionGroup[0].portal != null)
				{
					this.regionGroup[0].Room = Room.MakeNew(this.map);
				}
				else
				{
					ProfilerThreadCheck.BeginSample("Determine neighboring old rooms");
					Room room = null;
					bool flag = false;
					for (int m = 0; m < this.regionGroup.Count; m++)
					{
						foreach (Region current in this.regionGroup[m].NonPortalNeighbors)
						{
							if (current.Room != null)
							{
								if (room == null)
								{
									if (!this.reusedOldRooms.Contains(current.Room))
									{
										room = current.Room;
									}
								}
								else if (current.Room != room)
								{
									flag = true;
									if (current.Room.RegionCount > room.RegionCount && !this.reusedOldRooms.Contains(current.Room))
									{
										room = current.Room;
									}
								}
							}
						}
					}
					ProfilerThreadCheck.EndSample();
					ProfilerThreadCheck.BeginSample("Apply final result");
					if (room == null)
					{
						Room item = RegionTraverser.FloodAndSetRooms(this.regionGroup[0], this.map, null);
						this.newRooms.Add(item);
					}
					else if (!flag)
					{
						for (int n = 0; n < this.regionGroup.Count; n++)
						{
							this.regionGroup[n].Room = room;
						}
						this.reusedOldRooms.Add(room);
					}
					else
					{
						RegionTraverser.FloodAndSetRooms(this.regionGroup[0], this.map, room);
						this.reusedOldRooms.Add(room);
					}
					ProfilerThreadCheck.EndSample();
				}
			}
			ProfilerThreadCheck.EndSample();
			foreach (Room current2 in this.reusedOldRooms)
			{
				current2.RoomChanged();
			}
			for (int num2 = 0; num2 < this.newRooms.Count; num2++)
			{
				Room room2 = this.newRooms[num2];
				room2.RoomChanged();
				float temperature;
				if (this.map.temperatureCache.TryGetAverageCachedRoomTemp(room2, out temperature))
				{
					room2.Temperature = temperature;
				}
			}
			this.newRegions.Clear();
			this.newRooms.Clear();
			this.reusedOldRooms.Clear();
			this.map.regionDirtyer.SetAllClean();
			this.initialized = true;
			this.working = false;
			ProfilerThreadCheck.EndSample();
		}
	}
}
