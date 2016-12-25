using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RegionAndRoomUpdater
	{
		private static List<Region> newRegions = new List<Region>();

		private static HashSet<Room> reusedOldRooms = new HashSet<Room>();

		private static List<Room> newRooms = new List<Room>();

		private static List<Region> regionGroup = new List<Region>();

		private static bool initialized = false;

		private static bool working = false;

		private static bool enabledInt = true;

		public static bool Enabled
		{
			get
			{
				return RegionAndRoomUpdater.enabledInt;
			}
			set
			{
				RegionAndRoomUpdater.enabledInt = value;
			}
		}

		public static void Reinit()
		{
			RegionAndRoomUpdater.initialized = false;
		}

		public static void RebuildAllRegionsAndRooms()
		{
			Find.Map.temperatureCache.ResetTemperatureCache();
			RegionDirtyer.SetAllDirty();
			RegionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
		}

		public static void RebuildDirtyRegionsAndRooms()
		{
			if (RegionAndRoomUpdater.working || !RegionAndRoomUpdater.Enabled)
			{
				return;
			}
			RegionAndRoomUpdater.working = true;
			if (!RegionAndRoomUpdater.initialized)
			{
				RegionAndRoomUpdater.RebuildAllRegionsAndRooms();
			}
			if (!RegionDirtyer.AnyDirty)
			{
				RegionAndRoomUpdater.working = false;
				return;
			}
			ProfilerThreadCheck.BeginSample("RebuildDirtyRegionsAndRooms");
			ProfilerThreadCheck.BeginSample("Regenerate new regions from dirty cells");
			for (int i = 0; i < RegionDirtyer.DirtyCells.Count; i++)
			{
				IntVec3 intVec = RegionDirtyer.DirtyCells[i];
				if (Find.RegionGrid.GetValidRegionAt(intVec) == null)
				{
					Region region = RegionMaker.TryGenerateRegionFrom(intVec);
					if (region != null)
					{
						RegionAndRoomUpdater.newRegions.Add(region);
					}
				}
			}
			ProfilerThreadCheck.EndSample();
			ProfilerThreadCheck.BeginSample("Combine new regions into contiguous groups");
			int num = 0;
			for (int j = 0; j < RegionAndRoomUpdater.newRegions.Count; j++)
			{
				if (RegionAndRoomUpdater.newRegions[j].newRegionGroupIndex < 0)
				{
					RegionTraverser.FloodAndSetNewRegionIndex(RegionAndRoomUpdater.newRegions[j], num);
					num++;
				}
			}
			ProfilerThreadCheck.EndSample();
			ProfilerThreadCheck.BeginSample("Process " + num + " new region groups");
			for (int k = 0; k < num; k++)
			{
				ProfilerThreadCheck.BeginSample("Remake regionGroup list");
				RegionAndRoomUpdater.regionGroup.Clear();
				for (int l = 0; l < RegionAndRoomUpdater.newRegions.Count; l++)
				{
					if (RegionAndRoomUpdater.newRegions[l].newRegionGroupIndex == k)
					{
						RegionAndRoomUpdater.regionGroup.Add(RegionAndRoomUpdater.newRegions[l]);
					}
				}
				ProfilerThreadCheck.EndSample();
				if (RegionAndRoomUpdater.regionGroup.Count == 1 && RegionAndRoomUpdater.regionGroup[0].portal != null)
				{
					RegionAndRoomUpdater.regionGroup[0].Room = Room.MakeNew();
				}
				else
				{
					ProfilerThreadCheck.BeginSample("Determine neighboring old rooms");
					Room room = null;
					bool flag = false;
					for (int m = 0; m < RegionAndRoomUpdater.regionGroup.Count; m++)
					{
						foreach (Region current in RegionAndRoomUpdater.regionGroup[m].NonPortalNeighbors)
						{
							if (current.Room != null)
							{
								if (room == null)
								{
									if (!RegionAndRoomUpdater.reusedOldRooms.Contains(current.Room))
									{
										room = current.Room;
									}
								}
								else if (current.Room != room)
								{
									flag = true;
									if (current.Room.RegionCount > room.RegionCount && !RegionAndRoomUpdater.reusedOldRooms.Contains(current.Room))
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
						Room item = RegionTraverser.FloodAndSetRooms(RegionAndRoomUpdater.regionGroup[0], null);
						RegionAndRoomUpdater.newRooms.Add(item);
					}
					else if (!flag)
					{
						for (int n = 0; n < RegionAndRoomUpdater.regionGroup.Count; n++)
						{
							RegionAndRoomUpdater.regionGroup[n].Room = room;
						}
						RegionAndRoomUpdater.reusedOldRooms.Add(room);
					}
					else
					{
						RegionTraverser.FloodAndSetRooms(RegionAndRoomUpdater.regionGroup[0], room);
						RegionAndRoomUpdater.reusedOldRooms.Add(room);
					}
					ProfilerThreadCheck.EndSample();
				}
			}
			ProfilerThreadCheck.EndSample();
			foreach (Room current2 in RegionAndRoomUpdater.reusedOldRooms)
			{
				current2.RoomChanged();
			}
			for (int num2 = 0; num2 < RegionAndRoomUpdater.newRooms.Count; num2++)
			{
				Room room2 = RegionAndRoomUpdater.newRooms[num2];
				room2.RoomChanged();
				float temperature;
				if (Find.Map.temperatureCache.TryGetAverageCachedRoomTemp(room2, out temperature))
				{
					room2.Temperature = temperature;
				}
			}
			RegionAndRoomUpdater.newRegions.Clear();
			RegionAndRoomUpdater.newRooms.Clear();
			RegionAndRoomUpdater.reusedOldRooms.Clear();
			RegionDirtyer.SetAllClean();
			RegionAndRoomUpdater.initialized = true;
			RegionAndRoomUpdater.working = false;
			ProfilerThreadCheck.EndSample();
		}
	}
}
