using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class RegionGrid
	{
		private const int CleanSquaresPerFrame = 16;

		private Region[] regionGrid = new Region[CellIndices.NumGridCells];

		private int curCleanIndex;

		public List<Room> allRooms = new List<Room>();

		public static HashSet<Region> allRegionsYielded = new HashSet<Region>();

		public HashSet<Region> drawnRegions = new HashSet<Region>();

		public IEnumerable<Region> AllRegions
		{
			get
			{
				RegionGrid.allRegionsYielded.Clear();
				int count = CellIndices.NumGridCells;
				for (int i = 0; i < count; i++)
				{
					if (this.regionGrid[i] != null && !RegionGrid.allRegionsYielded.Contains(this.regionGrid[i]))
					{
						yield return this.regionGrid[i];
						RegionGrid.allRegionsYielded.Add(this.regionGrid[i]);
					}
				}
			}
		}

		public Region GetValidRegionAt(IntVec3 c)
		{
			if (!c.InBounds())
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
				return null;
			}
			RegionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
			Region region = this.regionGrid[CellIndices.CellToIndex(c)];
			if (region != null && region.valid)
			{
				return region;
			}
			return null;
		}

		public Region GetValidRegionAt_NoRebuild(IntVec3 c)
		{
			if (!c.InBounds())
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
				return null;
			}
			Region region = this.regionGrid[CellIndices.CellToIndex(c)];
			if (region != null && region.valid)
			{
				return region;
			}
			return null;
		}

		public Region GetRegionAt_InvalidAllowed(IntVec3 c)
		{
			return this.regionGrid[CellIndices.CellToIndex(c)];
		}

		public void SetRegionAt(IntVec3 c, Region reg)
		{
			this.regionGrid[CellIndices.CellToIndex(c)] = reg;
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
			if (DebugViewSettings.drawRegionTraversal)
			{
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				currentViewRect.ClipInsideMap();
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
			IntVec3 c = Gen.MouseCell();
			if (c.InBounds())
			{
				if (DebugViewSettings.drawRooms)
				{
					Room room = RoomQuery.RoomAt(c);
					if (room != null)
					{
						room.DebugDraw();
					}
				}
				if (DebugViewSettings.drawRegions || DebugViewSettings.drawRegionLinks)
				{
					Region regionAt_InvalidAllowed = this.GetRegionAt_InvalidAllowed(c);
					if (regionAt_InvalidAllowed != null)
					{
						regionAt_InvalidAllowed.DebugDrawMouseover();
					}
				}
			}
		}
	}
}
