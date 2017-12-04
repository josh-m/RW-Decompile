using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class RegionAndRoomUpdater
	{
		private Map map;

		private List<Region> newRegions = new List<Region>();

		private List<Room> newRooms = new List<Room>();

		private HashSet<Room> reusedOldRooms = new HashSet<Room>();

		private List<RoomGroup> newRoomGroups = new List<RoomGroup>();

		private HashSet<RoomGroup> reusedOldRoomGroups = new HashSet<RoomGroup>();

		private List<Region> currentRegionGroup = new List<Region>();

		private List<Room> currentRoomGroup = new List<Room>();

		private Stack<Room> tmpRoomStack = new Stack<Room>();

		private HashSet<Room> tmpVisitedRooms = new HashSet<Room>();

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

		public bool AnythingToRebuild
		{
			get
			{
				return this.map.regionDirtyer.AnyDirty || !this.initialized;
			}
		}

		public RegionAndRoomUpdater(Map map)
		{
			this.map = map;
		}

		public void RebuildAllRegionsAndRooms()
		{
			if (!this.Enabled)
			{
				Log.Warning("Called RebuildAllRegionsAndRooms() but RegionAndRoomUpdater is disabled. Regions won't be rebuilt.");
			}
			this.map.temperatureCache.ResetTemperatureCache();
			this.map.regionDirtyer.SetAllDirty();
			this.TryRebuildDirtyRegionsAndRooms();
		}

		public void TryRebuildDirtyRegionsAndRooms()
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
			try
			{
				this.RegenerateNewRegionsFromDirtyCells();
				this.CreateOrUpdateRooms();
			}
			catch (Exception arg)
			{
				Log.Error("Exception while rebuilding dirty regions: " + arg);
			}
			this.newRegions.Clear();
			this.map.regionDirtyer.SetAllClean();
			this.initialized = true;
			this.working = false;
			if (DebugSettings.detectRegionListersBugs)
			{
				Autotests_RegionListers.CheckBugs(this.map);
			}
		}

		private void RegenerateNewRegionsFromDirtyCells()
		{
			this.newRegions.Clear();
			List<IntVec3> dirtyCells = this.map.regionDirtyer.DirtyCells;
			for (int i = 0; i < dirtyCells.Count; i++)
			{
				IntVec3 intVec = dirtyCells[i];
				if (intVec.GetRegion(this.map, RegionType.Set_All) == null)
				{
					Region region = this.map.regionMaker.TryGenerateRegionFrom(intVec);
					if (region != null)
					{
						this.newRegions.Add(region);
					}
				}
			}
		}

		private void CreateOrUpdateRooms()
		{
			this.newRooms.Clear();
			this.reusedOldRooms.Clear();
			this.newRoomGroups.Clear();
			this.reusedOldRoomGroups.Clear();
			int numRegionGroups = this.CombineNewRegionsIntoContiguousGroups();
			this.CreateOrAttachToExistingRooms(numRegionGroups);
			int numRoomGroups = this.CombineNewAndReusedRoomsIntoContiguousGroups();
			this.CreateOrAttachToExistingRoomGroups(numRoomGroups);
			this.NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperature();
			this.newRooms.Clear();
			this.reusedOldRooms.Clear();
			this.newRoomGroups.Clear();
			this.reusedOldRoomGroups.Clear();
		}

		private int CombineNewRegionsIntoContiguousGroups()
		{
			int num = 0;
			for (int i = 0; i < this.newRegions.Count; i++)
			{
				if (this.newRegions[i].newRegionGroupIndex < 0)
				{
					RegionTraverser.FloodAndSetNewRegionIndex(this.newRegions[i], num);
					num++;
				}
			}
			return num;
		}

		private void CreateOrAttachToExistingRooms(int numRegionGroups)
		{
			for (int i = 0; i < numRegionGroups; i++)
			{
				this.currentRegionGroup.Clear();
				for (int j = 0; j < this.newRegions.Count; j++)
				{
					if (this.newRegions[j].newRegionGroupIndex == i)
					{
						this.currentRegionGroup.Add(this.newRegions[j]);
					}
				}
				if (!this.currentRegionGroup[0].type.AllowsMultipleRegionsPerRoom())
				{
					if (this.currentRegionGroup.Count != 1)
					{
						Log.Error("Region type doesn't allow multiple regions per room but there are >1 regions in this group.");
					}
					Room room = Room.MakeNew(this.map);
					this.currentRegionGroup[0].Room = room;
					this.newRooms.Add(room);
				}
				else
				{
					bool flag;
					Room room2 = this.FindCurrentRegionGroupNeighborWithMostRegions(out flag);
					if (room2 == null)
					{
						Room item = RegionTraverser.FloodAndSetRooms(this.currentRegionGroup[0], this.map, null);
						this.newRooms.Add(item);
					}
					else if (!flag)
					{
						for (int k = 0; k < this.currentRegionGroup.Count; k++)
						{
							this.currentRegionGroup[k].Room = room2;
						}
						this.reusedOldRooms.Add(room2);
					}
					else
					{
						RegionTraverser.FloodAndSetRooms(this.currentRegionGroup[0], this.map, room2);
						this.reusedOldRooms.Add(room2);
					}
				}
			}
		}

		private int CombineNewAndReusedRoomsIntoContiguousGroups()
		{
			int num = 0;
			foreach (Room current in this.reusedOldRooms)
			{
				current.newOrReusedRoomGroupIndex = -1;
			}
			foreach (Room current2 in this.reusedOldRooms.Concat(this.newRooms))
			{
				if (current2.newOrReusedRoomGroupIndex < 0)
				{
					this.tmpRoomStack.Clear();
					this.tmpRoomStack.Push(current2);
					current2.newOrReusedRoomGroupIndex = num;
					while (this.tmpRoomStack.Count != 0)
					{
						Room room = this.tmpRoomStack.Pop();
						foreach (Room current3 in room.Neighbors)
						{
							if (current3.newOrReusedRoomGroupIndex < 0 && this.ShouldBeInTheSameRoomGroup(room, current3))
							{
								current3.newOrReusedRoomGroupIndex = num;
								this.tmpRoomStack.Push(current3);
							}
						}
					}
					this.tmpRoomStack.Clear();
					num++;
				}
			}
			return num;
		}

		private void CreateOrAttachToExistingRoomGroups(int numRoomGroups)
		{
			for (int i = 0; i < numRoomGroups; i++)
			{
				this.currentRoomGroup.Clear();
				foreach (Room current in this.reusedOldRooms)
				{
					if (current.newOrReusedRoomGroupIndex == i)
					{
						this.currentRoomGroup.Add(current);
					}
				}
				for (int j = 0; j < this.newRooms.Count; j++)
				{
					if (this.newRooms[j].newOrReusedRoomGroupIndex == i)
					{
						this.currentRoomGroup.Add(this.newRooms[j]);
					}
				}
				bool flag;
				RoomGroup roomGroup = this.FindCurrentRoomGroupNeighborWithMostRegions(out flag);
				if (roomGroup == null)
				{
					RoomGroup roomGroup2 = RoomGroup.MakeNew(this.map);
					this.FloodAndSetRoomGroups(this.currentRoomGroup[0], roomGroup2);
					this.newRoomGroups.Add(roomGroup2);
				}
				else if (!flag)
				{
					for (int k = 0; k < this.currentRoomGroup.Count; k++)
					{
						this.currentRoomGroup[k].Group = roomGroup;
					}
					this.reusedOldRoomGroups.Add(roomGroup);
				}
				else
				{
					this.FloodAndSetRoomGroups(this.currentRoomGroup[0], roomGroup);
					this.reusedOldRoomGroups.Add(roomGroup);
				}
			}
		}

		private void FloodAndSetRoomGroups(Room start, RoomGroup roomGroup)
		{
			this.tmpRoomStack.Clear();
			this.tmpRoomStack.Push(start);
			this.tmpVisitedRooms.Clear();
			this.tmpVisitedRooms.Add(start);
			while (this.tmpRoomStack.Count != 0)
			{
				Room room = this.tmpRoomStack.Pop();
				room.Group = roomGroup;
				foreach (Room current in room.Neighbors)
				{
					if (!this.tmpVisitedRooms.Contains(current) && this.ShouldBeInTheSameRoomGroup(room, current))
					{
						this.tmpRoomStack.Push(current);
						this.tmpVisitedRooms.Add(current);
					}
				}
			}
			this.tmpVisitedRooms.Clear();
			this.tmpRoomStack.Clear();
		}

		private void NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperature()
		{
			foreach (Room current in this.reusedOldRooms)
			{
				current.Notify_RoomShapeOrContainedBedsChanged();
			}
			for (int i = 0; i < this.newRooms.Count; i++)
			{
				this.newRooms[i].Notify_RoomShapeOrContainedBedsChanged();
			}
			foreach (RoomGroup current2 in this.reusedOldRoomGroups)
			{
				current2.Notify_RoomGroupShapeChanged();
			}
			for (int j = 0; j < this.newRoomGroups.Count; j++)
			{
				RoomGroup roomGroup = this.newRoomGroups[j];
				roomGroup.Notify_RoomGroupShapeChanged();
				float temperature;
				if (this.map.temperatureCache.TryGetAverageCachedRoomGroupTemp(roomGroup, out temperature))
				{
					roomGroup.Temperature = temperature;
				}
			}
		}

		private Room FindCurrentRegionGroupNeighborWithMostRegions(out bool multipleOldNeighborRooms)
		{
			multipleOldNeighborRooms = false;
			Room room = null;
			for (int i = 0; i < this.currentRegionGroup.Count; i++)
			{
				foreach (Region current in this.currentRegionGroup[i].NeighborsOfSameType)
				{
					if (current.Room != null)
					{
						if (!this.reusedOldRooms.Contains(current.Room))
						{
							if (room == null)
							{
								room = current.Room;
							}
							else if (current.Room != room)
							{
								multipleOldNeighborRooms = true;
								if (current.Room.RegionCount > room.RegionCount)
								{
									room = current.Room;
								}
							}
						}
					}
				}
			}
			return room;
		}

		private RoomGroup FindCurrentRoomGroupNeighborWithMostRegions(out bool multipleOldNeighborRoomGroups)
		{
			multipleOldNeighborRoomGroups = false;
			RoomGroup roomGroup = null;
			for (int i = 0; i < this.currentRoomGroup.Count; i++)
			{
				foreach (Room current in this.currentRoomGroup[i].Neighbors)
				{
					if (current.Group != null && this.ShouldBeInTheSameRoomGroup(this.currentRoomGroup[i], current))
					{
						if (!this.reusedOldRoomGroups.Contains(current.Group))
						{
							if (roomGroup == null)
							{
								roomGroup = current.Group;
							}
							else if (current.Group != roomGroup)
							{
								multipleOldNeighborRoomGroups = true;
								if (current.Group.RegionCount > roomGroup.RegionCount)
								{
									roomGroup = current.Group;
								}
							}
						}
					}
				}
			}
			return roomGroup;
		}

		private bool ShouldBeInTheSameRoomGroup(Room a, Room b)
		{
			RegionType regionType = a.RegionType;
			RegionType regionType2 = b.RegionType;
			return (regionType == RegionType.Normal || regionType == RegionType.ImpassableFreeAirExchange) && (regionType2 == RegionType.Normal || regionType2 == RegionType.ImpassableFreeAirExchange);
		}
	}
}
