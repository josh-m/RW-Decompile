using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RegionMaker
	{
		private static Region newReg;

		private static int[] processedIndices = null;

		private static Queue<FloodFillRange> newClosedRanges = new Queue<FloodFillRange>(1000);

		private static int mapWidth;

		private static RegionGrid regionGrid;

		private static int[] leftEdgeOpenIDs;

		private static int[] rightEdgeOpenIDs;

		private static int[] bottomEdgeOpenIDs;

		private static int[] topEdgeOpenIDs;

		public static void Reinit()
		{
			RegionMaker.processedIndices = null;
			RegionMaker.newClosedRanges.Clear();
			RegionMaker.regionGrid = null;
			RegionMaker.leftEdgeOpenIDs = new int[Find.Map.Size.z];
			RegionMaker.rightEdgeOpenIDs = new int[Find.Map.Size.z];
			RegionMaker.bottomEdgeOpenIDs = new int[Find.Map.Size.x];
			RegionMaker.topEdgeOpenIDs = new int[Find.Map.Size.x];
		}

		public static Region TryGenerateRegionFrom(IntVec3 root)
		{
			if (!root.Walkable())
			{
				return null;
			}
			RegionMaker.regionGrid = Find.RegionGrid;
			Thing edifice = root.GetEdifice();
			if (edifice == null || !edifice.def.regionBarrier)
			{
				RegionMaker.newReg = Region.MakeNewUnfilled(root);
				RegionMaker.FloodRegionFrom(root);
				RegionMaker.AddNeighborThings();
				RegionMaker.ResolveSpanLinks();
				return RegionMaker.newReg;
			}
			Building_Door building_Door = edifice as Building_Door;
			if (building_Door != null)
			{
				return RegionMaker.MakePortalRegion(building_Door);
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to generate region in walkable cell with non-door building. root=",
					root,
					" building=",
					edifice
				}));
			}
			return null;
		}

		private static void AddNeighborThings()
		{
			CellRect cellRect = RegionMaker.newReg.extentsLimit;
			cellRect = cellRect.ExpandedBy(1);
			cellRect.ClipInsideMap();
			foreach (IntVec3 current in cellRect)
			{
				if (RegionMaker.regionGrid.GetValidRegionAt(current) == RegionMaker.newReg)
				{
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c = current + GenAdj.AdjacentCells[i];
						if (c.InBounds())
						{
							if (!c.Walkable())
							{
								RegionMaker.AddThingsInCellToLister_Neigh(c);
							}
						}
					}
				}
			}
		}

		private static void AddCell(IntVec3 c)
		{
			RegionMaker.regionGrid.SetRegionAt(c, RegionMaker.newReg);
			if (c.x < RegionMaker.newReg.extentsClose.minX)
			{
				RegionMaker.newReg.extentsClose.minX = c.x;
			}
			if (c.x > RegionMaker.newReg.extentsClose.maxX)
			{
				RegionMaker.newReg.extentsClose.maxX = c.x;
			}
			if (c.z < RegionMaker.newReg.extentsClose.minZ)
			{
				RegionMaker.newReg.extentsClose.minZ = c.z;
			}
			if (c.z > RegionMaker.newReg.extentsClose.maxZ)
			{
				RegionMaker.newReg.extentsClose.maxZ = c.z;
			}
			RegionMaker.AddThingsInCellToLister(c);
		}

		private static void AddThingsInCellToLister(IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if (!RegionMaker.newReg.ListerThings.Contains(thing) && (thing.Position == c || thing.def.regionBarrier))
				{
					RegionMaker.newReg.ListerThings.Add(thing);
				}
			}
		}

		private static void AddThingsInCellToLister_Neigh(IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if (thing.def.category != ThingCategory.Pawn && !RegionMaker.newReg.ListerThings.Contains(thing) && (thing.Position == c || thing.def.regionBarrier))
				{
					RegionMaker.newReg.ListerThings.Add(thing);
				}
			}
		}

		private static Region MakePortalRegion(Building_Door door)
		{
			IntVec3 position = door.Position;
			RegionMaker.newReg = Region.MakeNewUnfilled(position);
			RegionMaker.newReg.portal = door;
			RegionMaker.AddCell(position);
			if ((position + IntVec3.East).Walkable())
			{
				EdgeSpan edgeSpan = new EdgeSpan(position + IntVec3.East, SpanDirection.North, 1);
				RegionMaker.FinalizeSpanIfValid(ref edgeSpan);
			}
			if ((position + IntVec3.North).Walkable())
			{
				EdgeSpan edgeSpan2 = new EdgeSpan(position + IntVec3.North, SpanDirection.East, 1);
				RegionMaker.FinalizeSpanIfValid(ref edgeSpan2);
			}
			if ((position + IntVec3.West).Walkable())
			{
				EdgeSpan edgeSpan3 = new EdgeSpan(position, SpanDirection.North, 1);
				RegionMaker.FinalizeSpanIfValid(ref edgeSpan3);
			}
			if ((position + IntVec3.South).Walkable())
			{
				EdgeSpan edgeSpan4 = new EdgeSpan(position, SpanDirection.East, 1);
				RegionMaker.FinalizeSpanIfValid(ref edgeSpan4);
			}
			return RegionMaker.newReg;
		}

		private static void FloodRegionFrom(IntVec3 root)
		{
			RegionMaker.mapWidth = Find.Map.Size.x;
			if (RegionMaker.processedIndices == null)
			{
				RegionMaker.processedIndices = new int[CellIndices.NumGridCells];
			}
			RegionMaker.newClosedRanges.Clear();
			ProfilerThreadCheck.BeginSample("FloodRegionFrom " + root);
			RegionMaker.FillRowFrom(root.x, root.z);
			while (RegionMaker.newClosedRanges.Count > 0)
			{
				FloodFillRange floodFillRange = RegionMaker.newClosedRanges.Dequeue();
				int num = floodFillRange.z - 1;
				int num2 = floodFillRange.z + 1;
				int num3 = CellIndices.CellToIndex(floodFillRange.minX, num2);
				int num4 = CellIndices.CellToIndex(floodFillRange.minX, num);
				for (int i = floodFillRange.minX; i <= floodFillRange.maxX; i++)
				{
					if (floodFillRange.z > RegionMaker.newReg.extentsLimit.minZ && !RegionMaker.IndexWasProcessed(num4) && RegionMaker.CheckRegionableAndProcessNeighbor(num4, Rot4.South))
					{
						RegionMaker.FillRowFrom(i, num);
					}
					if (floodFillRange.z < RegionMaker.newReg.extentsLimit.maxZ && !RegionMaker.IndexWasProcessed(num3) && RegionMaker.CheckRegionableAndProcessNeighbor(num3, Rot4.North))
					{
						RegionMaker.FillRowFrom(i, num2);
					}
					num3++;
					num4++;
				}
			}
			ProfilerThreadCheck.EndSample();
		}

		private static void FillRowFrom(int rootX, int rootZ)
		{
			int num = CellIndices.CellToIndex(rootX, rootZ);
			bool flag = rootZ == RegionMaker.newReg.extentsLimit.minZ;
			bool flag2 = rootZ == RegionMaker.newReg.extentsLimit.maxZ;
			int num2 = rootX;
			int num3 = num;
			while (true)
			{
				IntVec3 intVec = CellIndices.IndexToCell(num3);
				RegionMaker.AddCell(intVec);
				RegionMaker.processedIndices[num3] = RegionMaker.newReg.id;
				if (flag)
				{
					if (RegionMaker.CheckRegionableAndProcessNeighbor(intVec + IntVec3.South, Rot4.South))
					{
						RegionMaker.bottomEdgeOpenIDs[intVec.x] = RegionMaker.newReg.id;
					}
				}
				else if (flag2 && RegionMaker.CheckRegionableAndProcessNeighbor(intVec + IntVec3.North, Rot4.North))
				{
					RegionMaker.topEdgeOpenIDs[intVec.x] = RegionMaker.newReg.id;
				}
				num2--;
				num3--;
				if (num2 < RegionMaker.newReg.extentsLimit.minX)
				{
					break;
				}
				if (!RegionMaker.CheckRegionableAndProcessNeighbor(num3, Rot4.West))
				{
					goto Block_9;
				}
			}
			if (num2 >= 0 && RegionMaker.CheckRegionableAndProcessNeighbor(num3, Rot4.West))
			{
				RegionMaker.leftEdgeOpenIDs[rootZ] = RegionMaker.newReg.id;
			}
			else if (num2 < 0)
			{
				RegionMaker.newReg.touchesMapEdge = true;
			}
			Block_9:
			num2++;
			int num4 = rootX;
			num3 = num;
			while (true)
			{
				num4++;
				num3++;
				if (num4 > RegionMaker.newReg.extentsLimit.maxX)
				{
					break;
				}
				if (!RegionMaker.CheckRegionableAndProcessNeighbor(num3, Rot4.East))
				{
					goto Block_14;
				}
				IntVec3 intVec2 = CellIndices.IndexToCell(num3);
				RegionMaker.AddCell(intVec2);
				RegionMaker.processedIndices[num3] = RegionMaker.newReg.id;
				if (flag)
				{
					if (RegionMaker.CheckRegionableAndProcessNeighbor(intVec2 + IntVec3.South, Rot4.South))
					{
						RegionMaker.bottomEdgeOpenIDs[intVec2.x] = RegionMaker.newReg.id;
					}
				}
				else if (flag2 && RegionMaker.CheckRegionableAndProcessNeighbor(intVec2 + IntVec3.North, Rot4.North))
				{
					RegionMaker.topEdgeOpenIDs[intVec2.x] = RegionMaker.newReg.id;
				}
			}
			if (num4 < RegionMaker.mapWidth && RegionMaker.CheckRegionableAndProcessNeighbor(num3, Rot4.East))
			{
				RegionMaker.rightEdgeOpenIDs[rootZ] = RegionMaker.newReg.id;
			}
			else if (num4 >= RegionMaker.mapWidth)
			{
				RegionMaker.newReg.touchesMapEdge = true;
			}
			Block_14:
			num4--;
			FloodFillRange item = new FloodFillRange(num2, num4, rootZ);
			RegionMaker.newClosedRanges.Enqueue(item);
		}

		private static bool CheckRegionableAndProcessNeighbor(int index, Rot4 processingDirection)
		{
			return RegionMaker.CheckRegionableAndProcessNeighbor(CellIndices.IndexToCell(index), processingDirection);
		}

		private static bool CheckRegionableAndProcessNeighbor(IntVec3 c, Rot4 processingDirection)
		{
			if (!c.InBounds())
			{
				RegionMaker.newReg.touchesMapEdge = true;
				return false;
			}
			if (!c.Walkable())
			{
				return false;
			}
			Thing regionBarrier = c.GetRegionBarrier();
			if (regionBarrier != null)
			{
				if (regionBarrier.def.IsDoor)
				{
					RegionMaker.TryMakePortalSpan(c, processingDirection);
				}
				return false;
			}
			return true;
		}

		private static void TryMakePortalSpan(IntVec3 c, Rot4 processingDirection)
		{
			EdgeSpan edgeSpan;
			if (processingDirection == Rot4.West)
			{
				edgeSpan = new EdgeSpan(c + IntVec3.East, SpanDirection.North, 1);
			}
			else if (processingDirection == Rot4.East)
			{
				edgeSpan = new EdgeSpan(c, SpanDirection.North, 1);
			}
			else if (processingDirection == Rot4.North)
			{
				edgeSpan = new EdgeSpan(c, SpanDirection.East, 1);
			}
			else
			{
				edgeSpan = new EdgeSpan(c + IntVec3.North, SpanDirection.East, 1);
			}
			RegionMaker.FinalizeSpanIfValid(ref edgeSpan);
		}

		private static bool IndexWasProcessed(int index)
		{
			return RegionMaker.processedIndices[index] == RegionMaker.newReg.id;
		}

		private static void ResolveSpanLinks()
		{
			EdgeSpan edgeSpan = default(EdgeSpan);
			EdgeSpan edgeSpan2 = default(EdgeSpan);
			for (int i = RegionMaker.newReg.extentsLimit.minZ; i <= RegionMaker.newReg.extentsLimit.maxZ; i++)
			{
				if (RegionMaker.leftEdgeOpenIDs[i] == RegionMaker.newReg.id)
				{
					RegionMaker.MakeOrExpandSpan(ref edgeSpan, SpanDirection.North, RegionMaker.newReg.extentsLimit.minX, i);
				}
				else
				{
					RegionMaker.FinalizeSpanIfValid(ref edgeSpan);
				}
				if (RegionMaker.rightEdgeOpenIDs[i] == RegionMaker.newReg.id)
				{
					RegionMaker.MakeOrExpandSpan(ref edgeSpan2, SpanDirection.North, RegionMaker.newReg.extentsLimit.maxX + 1, i);
				}
				else
				{
					RegionMaker.FinalizeSpanIfValid(ref edgeSpan2);
				}
			}
			RegionMaker.FinalizeSpanIfValid(ref edgeSpan);
			RegionMaker.FinalizeSpanIfValid(ref edgeSpan2);
			EdgeSpan edgeSpan3 = default(EdgeSpan);
			EdgeSpan edgeSpan4 = default(EdgeSpan);
			for (int j = RegionMaker.newReg.extentsLimit.minX; j <= RegionMaker.newReg.extentsLimit.maxX; j++)
			{
				if (RegionMaker.bottomEdgeOpenIDs[j] == RegionMaker.newReg.id)
				{
					RegionMaker.MakeOrExpandSpan(ref edgeSpan3, SpanDirection.East, j, RegionMaker.newReg.extentsLimit.minZ);
				}
				else
				{
					RegionMaker.FinalizeSpanIfValid(ref edgeSpan3);
				}
				if (RegionMaker.topEdgeOpenIDs[j] == RegionMaker.newReg.id)
				{
					RegionMaker.MakeOrExpandSpan(ref edgeSpan4, SpanDirection.East, j, RegionMaker.newReg.extentsLimit.maxZ + 1);
				}
				else
				{
					RegionMaker.FinalizeSpanIfValid(ref edgeSpan4);
				}
			}
			RegionMaker.FinalizeSpanIfValid(ref edgeSpan3);
			RegionMaker.FinalizeSpanIfValid(ref edgeSpan4);
		}

		private static void MakeOrExpandSpan(ref EdgeSpan span, SpanDirection dir, int x, int z)
		{
			if (!span.IsValid)
			{
				span = default(EdgeSpan);
				span.dir = dir;
				span.root = new IntVec3(x, 0, z);
				span.length = 1;
			}
			else
			{
				span.length++;
			}
		}

		private static void FinalizeSpanIfValid(ref EdgeSpan span)
		{
			if (span.IsValid)
			{
				RegionLink regionLink = RegionLinkDatabase.LinkFrom(span);
				regionLink.Register(RegionMaker.newReg);
				RegionMaker.newReg.links.Add(regionLink);
				span = default(EdgeSpan);
			}
		}
	}
}
