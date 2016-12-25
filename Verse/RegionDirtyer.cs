using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RegionDirtyer
	{
		private static List<IntVec3> dirtyCells = new List<IntVec3>();

		private static List<Region> regionsToDirty = new List<Region>();

		public static bool AnyDirty
		{
			get
			{
				return RegionDirtyer.dirtyCells.Count > 0;
			}
		}

		public static List<IntVec3> DirtyCells
		{
			get
			{
				return RegionDirtyer.dirtyCells;
			}
		}

		internal static void Notify_WalkabilityChanged(IntVec3 c)
		{
			RegionDirtyer.regionsToDirty.Clear();
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (c2.InBounds())
				{
					Region regionAt_InvalidAllowed = Find.RegionGrid.GetRegionAt_InvalidAllowed(c2);
					if (regionAt_InvalidAllowed != null && regionAt_InvalidAllowed.valid)
					{
						Find.Map.temperatureCache.TryCacheRegionTempInfo(c, regionAt_InvalidAllowed);
						RegionDirtyer.regionsToDirty.Add(regionAt_InvalidAllowed);
					}
				}
			}
			for (int j = 0; j < RegionDirtyer.regionsToDirty.Count; j++)
			{
				RegionDirtyer.SetRegionDirty(RegionDirtyer.regionsToDirty[j]);
			}
			if (c.Walkable() && !RegionDirtyer.dirtyCells.Contains(c))
			{
				RegionDirtyer.dirtyCells.Add(c);
			}
		}

		internal static void Notify_BarrierSpawned(Thing b)
		{
			RegionDirtyer.regionsToDirty.Clear();
			CellRect.CellRectIterator iterator = b.OccupiedRect().ExpandedBy(1).ClipInsideMap().GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				Region validRegionAt_NoRebuild = Find.RegionGrid.GetValidRegionAt_NoRebuild(current);
				if (validRegionAt_NoRebuild != null)
				{
					Find.Map.temperatureCache.TryCacheRegionTempInfo(current, validRegionAt_NoRebuild);
					RegionDirtyer.regionsToDirty.Add(validRegionAt_NoRebuild);
				}
				iterator.MoveNext();
			}
			for (int i = 0; i < RegionDirtyer.regionsToDirty.Count; i++)
			{
				RegionDirtyer.SetRegionDirty(RegionDirtyer.regionsToDirty[i]);
			}
		}

		internal static void Notify_BarrierDespawned(Thing b)
		{
			RegionDirtyer.regionsToDirty.Clear();
			Region validRegionAt_NoRebuild = Find.RegionGrid.GetValidRegionAt_NoRebuild(b.Position);
			if (validRegionAt_NoRebuild != null)
			{
				Find.Map.temperatureCache.TryCacheRegionTempInfo(b.Position, validRegionAt_NoRebuild);
				RegionDirtyer.regionsToDirty.Add(validRegionAt_NoRebuild);
			}
			foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(b))
			{
				if (current.InBounds())
				{
					Region validRegionAt_NoRebuild2 = Find.RegionGrid.GetValidRegionAt_NoRebuild(current);
					if (validRegionAt_NoRebuild2 != null)
					{
						Find.Map.temperatureCache.TryCacheRegionTempInfo(current, validRegionAt_NoRebuild2);
						RegionDirtyer.regionsToDirty.Add(validRegionAt_NoRebuild2);
					}
				}
			}
			for (int i = 0; i < RegionDirtyer.regionsToDirty.Count; i++)
			{
				RegionDirtyer.SetRegionDirty(RegionDirtyer.regionsToDirty[i]);
			}
			if (b.def.size.x == 1 && b.def.size.z == 1)
			{
				RegionDirtyer.dirtyCells.Add(b.Position);
			}
			else
			{
				CellRect cellRect = b.OccupiedRect();
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					for (int k = cellRect.minX; k <= cellRect.maxX; k++)
					{
						IntVec3 item = new IntVec3(k, 0, j);
						RegionDirtyer.dirtyCells.Add(item);
					}
				}
			}
		}

		internal static void SetAllClean()
		{
			for (int i = 0; i < RegionDirtyer.dirtyCells.Count; i++)
			{
				Find.Map.temperatureCache.ResetCachedCellInfo(RegionDirtyer.dirtyCells[i]);
			}
			RegionDirtyer.dirtyCells.Clear();
		}

		private static void SetRegionDirty(Region reg)
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
			foreach (IntVec3 current in reg.Cells)
			{
				RegionDirtyer.dirtyCells.Add(current);
				if (DebugViewSettings.drawRegionDirties)
				{
					Find.DebugDrawer.FlashCell(current, 0f, null);
				}
			}
		}

		internal static void SetAllDirty()
		{
			RegionDirtyer.dirtyCells.Clear();
			foreach (IntVec3 current in Find.Map)
			{
				RegionDirtyer.dirtyCells.Add(current);
			}
			foreach (Region current2 in Find.RegionGrid.AllRegions)
			{
				current2.valid = false;
			}
		}
	}
}
