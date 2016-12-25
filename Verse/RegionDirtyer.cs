using System;
using System.Collections.Generic;

namespace Verse
{
	public class RegionDirtyer
	{
		private Map map;

		private List<IntVec3> dirtyCells = new List<IntVec3>();

		private List<Region> regionsToDirty = new List<Region>();

		public bool AnyDirty
		{
			get
			{
				return this.dirtyCells.Count > 0;
			}
		}

		public List<IntVec3> DirtyCells
		{
			get
			{
				return this.dirtyCells;
			}
		}

		public RegionDirtyer(Map map)
		{
			this.map = map;
		}

		internal void Notify_WalkabilityChanged(IntVec3 c)
		{
			this.regionsToDirty.Clear();
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (c2.InBounds(this.map))
				{
					Region regionAt_InvalidAllowed = this.map.regionGrid.GetRegionAt_InvalidAllowed(c2);
					if (regionAt_InvalidAllowed != null && regionAt_InvalidAllowed.valid)
					{
						this.map.temperatureCache.TryCacheRegionTempInfo(c, regionAt_InvalidAllowed);
						this.regionsToDirty.Add(regionAt_InvalidAllowed);
					}
				}
			}
			for (int j = 0; j < this.regionsToDirty.Count; j++)
			{
				this.SetRegionDirty(this.regionsToDirty[j], true);
			}
			this.regionsToDirty.Clear();
			if (c.Walkable(this.map) && !this.dirtyCells.Contains(c))
			{
				this.dirtyCells.Add(c);
			}
		}

		internal void Notify_BarrierSpawned(Thing b)
		{
			this.regionsToDirty.Clear();
			CellRect.CellRectIterator iterator = b.OccupiedRect().ExpandedBy(1).ClipInsideMap(b.Map).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				Region validRegionAt_NoRebuild = b.Map.regionGrid.GetValidRegionAt_NoRebuild(current);
				if (validRegionAt_NoRebuild != null)
				{
					b.Map.temperatureCache.TryCacheRegionTempInfo(current, validRegionAt_NoRebuild);
					this.regionsToDirty.Add(validRegionAt_NoRebuild);
				}
				iterator.MoveNext();
			}
			for (int i = 0; i < this.regionsToDirty.Count; i++)
			{
				this.SetRegionDirty(this.regionsToDirty[i], true);
			}
			this.regionsToDirty.Clear();
		}

		internal void Notify_BarrierDespawned(Thing b)
		{
			this.regionsToDirty.Clear();
			Region validRegionAt_NoRebuild = this.map.regionGrid.GetValidRegionAt_NoRebuild(b.Position);
			if (validRegionAt_NoRebuild != null)
			{
				this.map.temperatureCache.TryCacheRegionTempInfo(b.Position, validRegionAt_NoRebuild);
				this.regionsToDirty.Add(validRegionAt_NoRebuild);
			}
			foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(b))
			{
				if (current.InBounds(this.map))
				{
					Region validRegionAt_NoRebuild2 = this.map.regionGrid.GetValidRegionAt_NoRebuild(current);
					if (validRegionAt_NoRebuild2 != null)
					{
						this.map.temperatureCache.TryCacheRegionTempInfo(current, validRegionAt_NoRebuild2);
						this.regionsToDirty.Add(validRegionAt_NoRebuild2);
					}
				}
			}
			for (int i = 0; i < this.regionsToDirty.Count; i++)
			{
				this.SetRegionDirty(this.regionsToDirty[i], true);
			}
			this.regionsToDirty.Clear();
			if (b.def.size.x == 1 && b.def.size.z == 1)
			{
				this.dirtyCells.Add(b.Position);
			}
			else
			{
				CellRect cellRect = b.OccupiedRect();
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					for (int k = cellRect.minX; k <= cellRect.maxX; k++)
					{
						IntVec3 item = new IntVec3(k, 0, j);
						this.dirtyCells.Add(item);
					}
				}
			}
		}

		internal void SetAllClean()
		{
			for (int i = 0; i < this.dirtyCells.Count; i++)
			{
				this.map.temperatureCache.ResetCachedCellInfo(this.dirtyCells[i]);
			}
			this.dirtyCells.Clear();
		}

		private void SetRegionDirty(Region reg, bool addCellsToDirtyCells = true)
		{
			if (!reg.valid)
			{
				return;
			}
			reg.valid = false;
			reg.Room = null;
			for (int i = 0; i < reg.links.Count; i++)
			{
				reg.links[i].Deregister(reg);
			}
			reg.links.Clear();
			if (addCellsToDirtyCells)
			{
				foreach (IntVec3 current in reg.Cells)
				{
					this.dirtyCells.Add(current);
					if (DebugViewSettings.drawRegionDirties)
					{
						this.map.debugDrawer.FlashCell(current, 0f, null);
					}
				}
			}
		}

		internal void SetAllDirty()
		{
			this.dirtyCells.Clear();
			foreach (IntVec3 current in this.map)
			{
				this.dirtyCells.Add(current);
			}
			foreach (Region current2 in this.map.regionGrid.AllRegions)
			{
				this.SetRegionDirty(current2, false);
			}
		}
	}
}
