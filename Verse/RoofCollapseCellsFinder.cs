using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RoofCollapseCellsFinder
	{
		private static List<IntVec3> roofsCollapsingBecauseTooFar = new List<IntVec3>();

		private static HashSet<IntVec3> visitedCells = new HashSet<IntVec3>();

		public static void Notify_RoofHolderDespawned(Thing t, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(t.OccupiedRect(), map);
			RoofGrid roofGrid = map.roofGrid;
			RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Clear();
			for (int i = 0; i < RoofCollapseUtility.RoofSupportRadialCellsCount; i++)
			{
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					if (roofGrid.Roofed(intVec.x, intVec.z))
					{
						if (!map.roofCollapseBuffer.IsMarkedToCollapse(intVec))
						{
							if (!RoofCollapseUtility.WithinRangeOfRoofHolder(intVec, map))
							{
								map.roofCollapseBuffer.MarkToCollapse(intVec);
								RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Add(intVec);
							}
						}
					}
				}
			}
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar, map, false);
			RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Clear();
		}

		public static void CheckCollapseFlyingRoofs(List<IntVec3> nearCells, Map map, bool removalMode = false)
		{
			RoofCollapseCellsFinder.visitedCells.Clear();
			for (int i = 0; i < nearCells.Count; i++)
			{
				RoofCollapseCellsFinder.CheckCollapseFlyingRoofAtAndAdjInternal(nearCells[i], map, removalMode);
			}
			RoofCollapseCellsFinder.visitedCells.Clear();
		}

		public static void CheckCollapseFlyingRoofs(CellRect nearRect, Map map)
		{
			RoofCollapseCellsFinder.visitedCells.Clear();
			CellRect.CellRectIterator iterator = nearRect.GetIterator();
			while (!iterator.Done())
			{
				RoofCollapseCellsFinder.CheckCollapseFlyingRoofAtAndAdjInternal(iterator.Current, map, false);
				iterator.MoveNext();
			}
			RoofCollapseCellsFinder.visitedCells.Clear();
		}

		private static bool CheckCollapseFlyingRoofAtAndAdjInternal(IntVec3 root, Map map, bool removalMode)
		{
			ProfilerThreadCheck.BeginSample("CheckCollapseFlyingRoofAtInternal()");
			RoofCollapseBuffer roofCollapseBuffer = map.roofCollapseBuffer;
			if (removalMode && roofCollapseBuffer.CellsMarkedToCollapse.Count > 0)
			{
				map.roofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
			}
			for (int i = 0; i < 5; i++)
			{
				IntVec3 intVec = root + GenAdj.CardinalDirectionsAndInside[i];
				if (intVec.InBounds(map))
				{
					if (intVec.Roofed(map))
					{
						if (!RoofCollapseCellsFinder.visitedCells.Contains(intVec))
						{
							if (!roofCollapseBuffer.IsMarkedToCollapse(intVec))
							{
								if (!RoofCollapseCellsFinder.ConnectsToRoofHolder(intVec, map))
								{
									map.floodFiller.FloodFill(intVec, (IntVec3 x) => x.Roofed(map), delegate(IntVec3 x)
									{
										roofCollapseBuffer.MarkToCollapse(x);
									});
									if (removalMode)
									{
										for (int j = 0; j < roofCollapseBuffer.CellsMarkedToCollapse.Count; j++)
										{
											map.roofGrid.SetRoof(roofCollapseBuffer.CellsMarkedToCollapse[j], null);
										}
										roofCollapseBuffer.Clear();
									}
								}
							}
						}
					}
				}
			}
			ProfilerThreadCheck.EndSample();
			return false;
		}

		private static bool ConnectsToRoofHolder(IntVec3 c, Map map)
		{
			bool connected = false;
			map.floodFiller.FloodFill(c, (IntVec3 x) => x.Roofed(map) && !connected, delegate(IntVec3 x)
			{
				if (RoofCollapseCellsFinder.visitedCells.Contains(x))
				{
					connected = true;
					return;
				}
				RoofCollapseCellsFinder.visitedCells.Add(x);
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
					if (c2.InBounds(map))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.holdsRoof)
						{
							connected = true;
							break;
						}
					}
				}
			});
			return connected;
		}
	}
}
