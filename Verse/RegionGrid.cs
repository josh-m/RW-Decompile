using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class RegionGrid
	{
		private const int CleanSquaresPerFrame = 16;

		private Map map;

		private Region[] regionGrid;

		private int curCleanIndex;

		public List<Room> allRooms = new List<Room>();

		public static HashSet<Region> allRegionsYielded = new HashSet<Region>();

		public HashSet<Region> drawnRegions = new HashSet<Region>();

		public Region[] DirectGrid
		{
			get
			{
				if (!this.map.regionAndRoomUpdater.Enabled && this.map.regionAndRoomUpdater.AnythingToRebuild)
				{
					Log.Warning("Trying to get the region grid but RegionAndRoomUpdater is disabled. The result may be incorrect.");
				}
				this.map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
				return this.regionGrid;
			}
		}

		public IEnumerable<Region> AllRegions_NoRebuild_InvalidAllowed
		{
			get
			{
				RegionGrid.allRegionsYielded.Clear();
				try
				{
					int count = this.map.cellIndices.NumGridCells;
					for (int i = 0; i < count; i++)
					{
						if (this.regionGrid[i] != null && !RegionGrid.allRegionsYielded.Contains(this.regionGrid[i]))
						{
							yield return this.regionGrid[i];
						}
					}
				}
				finally
				{
					base.<>__Finally0();
				}
			}
		}

		public IEnumerable<Region> AllRegions
		{
			get
			{
				if (!this.map.regionAndRoomUpdater.Enabled && this.map.regionAndRoomUpdater.AnythingToRebuild)
				{
					Log.Warning("Trying to get all valid regions but RegionAndRoomUpdater is disabled. The result may be incorrect.");
				}
				this.map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
				RegionGrid.allRegionsYielded.Clear();
				try
				{
					int count = this.map.cellIndices.NumGridCells;
					for (int i = 0; i < count; i++)
					{
						if (this.regionGrid[i] != null && this.regionGrid[i].valid && !RegionGrid.allRegionsYielded.Contains(this.regionGrid[i]))
						{
							yield return this.regionGrid[i];
						}
					}
				}
				finally
				{
					base.<>__Finally0();
				}
			}
		}

		public RegionGrid(Map map)
		{
			this.map = map;
			this.regionGrid = new Region[map.cellIndices.NumGridCells];
		}

		public Region GetValidRegionAt(IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
				return null;
			}
			if (!this.map.regionAndRoomUpdater.Enabled && this.map.regionAndRoomUpdater.AnythingToRebuild)
			{
				Log.Warning("Trying to get valid region at " + c + " but RegionAndRoomUpdater is disabled. The result may be incorrect.");
			}
			this.map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
			Region region = this.regionGrid[this.map.cellIndices.CellToIndex(c)];
			if (region != null && region.valid)
			{
				return region;
			}
			return null;
		}

		public Region GetValidRegionAt_NoRebuild(IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
				return null;
			}
			Region region = this.regionGrid[this.map.cellIndices.CellToIndex(c)];
			if (region != null && region.valid)
			{
				return region;
			}
			return null;
		}

		public Region GetRegionAt_NoRebuild_InvalidAllowed(IntVec3 c)
		{
			return this.regionGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public void SetRegionAt(IntVec3 c, Region reg)
		{
			this.regionGrid[this.map.cellIndices.CellToIndex(c)] = reg;
		}

		public void UpdateClean()
		{
			for (int i = 0; i < 16; i++)
			{
				if (this.curCleanIndex >= this.regionGrid.Length)
				{
					this.curCleanIndex = 0;
				}
				Region region = this.regionGrid[this.curCleanIndex];
				if (region != null && !region.valid)
				{
					this.regionGrid[this.curCleanIndex] = null;
				}
				this.curCleanIndex++;
			}
		}

		public void DebugDraw()
		{
			if (this.map != Find.VisibleMap)
			{
				return;
			}
			if (DebugViewSettings.drawRegionTraversal)
			{
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				currentViewRect.ClipInsideMap(this.map);
				foreach (IntVec3 current in currentViewRect)
				{
					Region validRegionAt = this.GetValidRegionAt(current);
					if (validRegionAt != null && !this.drawnRegions.Contains(validRegionAt))
					{
						validRegionAt.DebugDraw();
						this.drawnRegions.Add(validRegionAt);
					}
				}
				this.drawnRegions.Clear();
			}
			IntVec3 intVec = UI.MouseCell();
			if (intVec.InBounds(this.map))
			{
				if (DebugViewSettings.drawRooms)
				{
					Room room = intVec.GetRoom(this.map, RegionType.Set_All);
					if (room != null)
					{
						room.DebugDraw();
					}
				}
				if (DebugViewSettings.drawRoomGroups)
				{
					RoomGroup roomGroup = intVec.GetRoomGroup(this.map);
					if (roomGroup != null)
					{
						roomGroup.DebugDraw();
					}
				}
				if (DebugViewSettings.drawRegions || DebugViewSettings.drawRegionLinks || DebugViewSettings.drawRegionThings)
				{
					Region regionAt_NoRebuild_InvalidAllowed = this.GetRegionAt_NoRebuild_InvalidAllowed(intVec);
					if (regionAt_NoRebuild_InvalidAllowed != null)
					{
						regionAt_NoRebuild_InvalidAllowed.DebugDrawMouseover();
					}
				}
			}
		}
	}
}
