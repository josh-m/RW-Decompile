using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RoofCollapseCellsFinder
	{
		private static List<IntVec3> roofsCollapsingBecauseTooFar = new List<IntVec3>();

		private static HashSet<IntVec3> visitedCells = new HashSet<IntVec3>();

		public static void Notify_RoofHolderDespawned(Thing t)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(t.OccupiedRect());
			RoofGrid roofGrid = Find.RoofGrid;
			RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Clear();
			for (int i = 0; i < RoofCollapseUtility.RoofSupportRadialCellsCount; i++)
			{
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds())
				{
					if (roofGrid.Roofed(intVec.x, intVec.z))
					{
						if (!RoofCollapseBuffer.IsMarkedToCollapse(intVec))
						{
							if (!RoofCollapseUtility.WithinRangeOfRoofHolder(intVec))
							{
								RoofCollapseBuffer.MarkToCollapse(intVec);
								RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Add(intVec);
							}
						}
					}
				}
			}
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar, false);
			RoofCollapseCellsFinder.roofsCollapsingBecauseTooFar.Clear();
		}

		public static void CheckCollapseFlyingRoofs(List<IntVec3> nearCells, bool removalMode = false)
		{
			RoofCollapseCellsFinder.visitedCells.Clear();
			for (int i = 0; i < nearCells.Count; i++)
			{
				RoofCollapseCellsFinder.CheckCollapseFlyingRoofAtAndAdjInternal(nearCells[i], removalMode);
			}
			RoofCollapseCellsFinder.visitedCells.Clear();
		}

		public static void CheckCollapseFlyingRoofs(CellRect nearRect)
		{
			RoofCollapseCellsFinder.visitedCells.Clear();
			CellRect.CellRectIterator iterator = nearRect.GetIterator();
			while (!iterator.Done())
			{
				RoofCollapseCellsFinder.CheckCollapseFlyingRoofAtAndAdjInternal(iterator.Current, false);
				iterator.MoveNext();
			}
			RoofCollapseCellsFinder.visitedCells.Clear();
		}

		private static bool CheckCollapseFlyingRoofAtAndAdjInternal(IntVec3 root, bool removalMode)
		{
			ProfilerThreadCheck.BeginSample("CheckCollapseFlyingRoofAtInternal()");
			if (removalMode && RoofCollapseBuffer.CellsMarkedToCollapse.Count > 0)
			{
				RoofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
			}
			for (int i = 0; i < 5; i++)
			{
				IntVec3 intVec = root + GenAdj.CardinalDirectionsAndInside[i];
				if (intVec.InBounds())
				{
					if (intVec.Roofed())
					{
						if (!RoofCollapseCellsFinder.visitedCells.Contains(intVec))
						{
							if (!RoofCollapseBuffer.IsMarkedToCollapse(intVec))
							{
								if (!RoofCollapseCellsFinder.ConnectsToRoofHolder(intVec))
								{
									FloodFiller.FloodFill(intVec, (IntVec3 x) => x.Roofed(), delegate(IntVec3 x)
									{
										RoofCollapseBuffer.MarkToCollapse(x);
									});
									if (removalMode)
									{
										for (int j = 0; j < RoofCollapseBuffer.CellsMarkedToCollapse.Count; j++)
										{
											Find.RoofGrid.SetRoof(RoofCollapseBuffer.CellsMarkedToCollapse[j], null);
										}
										RoofCollapseBuffer.Clear();
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

		private static bool ConnectsToRoofHolder(IntVec3 c)
		{
			bool connected = false;
			FloodFiller.FloodFill(c, (IntVec3 x) => x.Roofed() && !connected, delegate(IntVec3 x)
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
					if (c2.InBounds())
					{
						Building edifice = c2.GetEdifice();
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
