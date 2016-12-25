using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class RegionMaker
	{
		private Map map;

		private Region newReg;

		private int[] processedIndices;

		private Queue<FloodFillRange> newClosedRanges = new Queue<FloodFillRange>(1000);

		private bool working;

		private int mapWidth;

		private RegionGrid regionGrid;

		private int[] leftEdgeOpenIDs;

		private int[] rightEdgeOpenIDs;

		private int[] bottomEdgeOpenIDs;

		private int[] topEdgeOpenIDs;

		public RegionMaker(Map map)
		{
			this.map = map;
			this.leftEdgeOpenIDs = new int[map.Size.z];
			this.rightEdgeOpenIDs = new int[map.Size.z];
			this.bottomEdgeOpenIDs = new int[map.Size.x];
			this.topEdgeOpenIDs = new int[map.Size.x];
		}

		public Region TryGenerateRegionFrom(IntVec3 root)
		{
			if (!root.Walkable(this.map))
			{
				return null;
			}
			if (this.working)
			{
				Log.Error("Trying to generate a new region but we are currently generating one. Nested calls are not allowed.");
				return null;
			}
			this.working = true;
			Region result;
			try
			{
				this.regionGrid = this.map.regionGrid;
				Thing edifice = root.GetEdifice(this.map);
				if (edifice != null && edifice.def.regionBarrier)
				{
					Building_Door building_Door = edifice as Building_Door;
					if (building_Door != null)
					{
						result = this.MakePortalRegion(building_Door);
					}
					else
					{
						if (Current.ProgramState == ProgramState.Playing)
						{
							Log.Error(string.Concat(new object[]
							{
								"Tried to generate region in walkable cell with non-door building. root=",
								root,
								" building=",
								edifice
							}));
						}
						result = null;
					}
				}
				else
				{
					this.newReg = Region.MakeNewUnfilled(root, this.map);
					this.FloodRegionFrom(root);
					this.AddNeighborThings();
					this.ResolveSpanLinks();
					result = this.newReg;
				}
			}
			finally
			{
				this.working = false;
			}
			return result;
		}

		private void AddNeighborThings()
		{
			CellRect cellRect = this.newReg.extentsLimit;
			cellRect = cellRect.ExpandedBy(1);
			cellRect.ClipInsideMap(this.map);
			foreach (IntVec3 current in cellRect)
			{
				if (this.regionGrid.GetValidRegionAt(current) == this.newReg)
				{
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c = current + GenAdj.AdjacentCells[i];
						if (c.InBounds(this.map))
						{
							if (!c.Walkable(this.map))
							{
								this.AddThingsInCellToLister_Neigh(c);
							}
						}
					}
				}
			}
		}

		private void AddCell(IntVec3 c)
		{
			this.regionGrid.SetRegionAt(c, this.newReg);
			if (c.x < this.newReg.extentsClose.minX)
			{
				this.newReg.extentsClose.minX = c.x;
			}
			if (c.x > this.newReg.extentsClose.maxX)
			{
				this.newReg.extentsClose.maxX = c.x;
			}
			if (c.z < this.newReg.extentsClose.minZ)
			{
				this.newReg.extentsClose.minZ = c.z;
			}
			if (c.z > this.newReg.extentsClose.maxZ)
			{
				this.newReg.extentsClose.maxZ = c.z;
			}
			this.AddThingsInCellToLister(c);
		}

		private void AddThingsInCellToLister(IntVec3 c)
		{
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if (!this.newReg.ListerThings.Contains(thing) && (thing.Position == c || thing.def.regionBarrier))
				{
					this.newReg.ListerThings.Add(thing);
				}
			}
		}

		private void AddThingsInCellToLister_Neigh(IntVec3 c)
		{
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if (thing.def.category != ThingCategory.Pawn && !this.newReg.ListerThings.Contains(thing) && (thing.Position == c || thing.def.regionBarrier))
				{
					this.newReg.ListerThings.Add(thing);
				}
			}
		}

		private Region MakePortalRegion(Building_Door door)
		{
			IntVec3 position = door.Position;
			this.newReg = Region.MakeNewUnfilled(position, door.Map);
			this.newReg.portal = door;
			this.AddCell(position);
			if ((position + IntVec3.East).Walkable(this.map))
			{
				EdgeSpan edgeSpan = new EdgeSpan(position + IntVec3.East, SpanDirection.North, 1);
				this.FinalizeSpanIfValid(ref edgeSpan);
			}
			if ((position + IntVec3.North).Walkable(this.map))
			{
				EdgeSpan edgeSpan2 = new EdgeSpan(position + IntVec3.North, SpanDirection.East, 1);
				this.FinalizeSpanIfValid(ref edgeSpan2);
			}
			if ((position + IntVec3.West).Walkable(this.map))
			{
				EdgeSpan edgeSpan3 = new EdgeSpan(position, SpanDirection.North, 1);
				this.FinalizeSpanIfValid(ref edgeSpan3);
			}
			if ((position + IntVec3.South).Walkable(this.map))
			{
				EdgeSpan edgeSpan4 = new EdgeSpan(position, SpanDirection.East, 1);
				this.FinalizeSpanIfValid(ref edgeSpan4);
			}
			return this.newReg;
		}

		private void FloodRegionFrom(IntVec3 root)
		{
			CellIndices cellIndices = this.map.cellIndices;
			this.mapWidth = this.map.Size.x;
			if (this.processedIndices == null)
			{
				this.processedIndices = new int[cellIndices.NumGridCells];
			}
			this.newClosedRanges.Clear();
			ProfilerThreadCheck.BeginSample("FloodRegionFrom " + root);
			this.FillRowFrom(root.x, root.z);
			while (this.newClosedRanges.Count > 0)
			{
				FloodFillRange floodFillRange = this.newClosedRanges.Dequeue();
				int num = floodFillRange.z - 1;
				int num2 = floodFillRange.z + 1;
				int num3 = cellIndices.CellToIndex(floodFillRange.minX, num2);
				int num4 = cellIndices.CellToIndex(floodFillRange.minX, num);
				for (int i = floodFillRange.minX; i <= floodFillRange.maxX; i++)
				{
					if (floodFillRange.z > this.newReg.extentsLimit.minZ && !this.IndexWasProcessed(num4) && this.CheckRegionableAndProcessNeighbor(num4, Rot4.South))
					{
						this.FillRowFrom(i, num);
					}
					if (floodFillRange.z < this.newReg.extentsLimit.maxZ && !this.IndexWasProcessed(num3) && this.CheckRegionableAndProcessNeighbor(num3, Rot4.North))
					{
						this.FillRowFrom(i, num2);
					}
					num3++;
					num4++;
				}
			}
			ProfilerThreadCheck.EndSample();
		}

		private void FillRowFrom(int rootX, int rootZ)
		{
			CellIndices cellIndices = this.map.cellIndices;
			int num = cellIndices.CellToIndex(rootX, rootZ);
			bool flag = rootZ == this.newReg.extentsLimit.minZ;
			bool flag2 = rootZ == this.newReg.extentsLimit.maxZ;
			int num2 = rootX;
			int num3 = num;
			while (true)
			{
				IntVec3 intVec = cellIndices.IndexToCell(num3);
				this.AddCell(intVec);
				this.processedIndices[num3] = this.newReg.id;
				if (flag)
				{
					if (this.CheckRegionableAndProcessNeighbor(intVec + IntVec3.South, Rot4.South))
					{
						this.bottomEdgeOpenIDs[intVec.x] = this.newReg.id;
					}
				}
				else if (flag2 && this.CheckRegionableAndProcessNeighbor(intVec + IntVec3.North, Rot4.North))
				{
					this.topEdgeOpenIDs[intVec.x] = this.newReg.id;
				}
				num2--;
				num3--;
				if (num2 < this.newReg.extentsLimit.minX)
				{
					break;
				}
				if (!this.CheckRegionableAndProcessNeighbor(num3, Rot4.West))
				{
					goto Block_9;
				}
			}
			if (num2 >= 0 && this.CheckRegionableAndProcessNeighbor(num3, Rot4.West))
			{
				this.leftEdgeOpenIDs[rootZ] = this.newReg.id;
			}
			else if (num2 < 0)
			{
				this.newReg.touchesMapEdge = true;
			}
			Block_9:
			num2++;
			int num4 = rootX;
			num3 = num;
			while (true)
			{
				num4++;
				num3++;
				if (num4 > this.newReg.extentsLimit.maxX)
				{
					break;
				}
				if (!this.CheckRegionableAndProcessNeighbor(num3, Rot4.East))
				{
					goto Block_14;
				}
				IntVec3 intVec2 = cellIndices.IndexToCell(num3);
				this.AddCell(intVec2);
				this.processedIndices[num3] = this.newReg.id;
				if (flag)
				{
					if (this.CheckRegionableAndProcessNeighbor(intVec2 + IntVec3.South, Rot4.South))
					{
						this.bottomEdgeOpenIDs[intVec2.x] = this.newReg.id;
					}
				}
				else if (flag2 && this.CheckRegionableAndProcessNeighbor(intVec2 + IntVec3.North, Rot4.North))
				{
					this.topEdgeOpenIDs[intVec2.x] = this.newReg.id;
				}
			}
			if (num4 < this.mapWidth && this.CheckRegionableAndProcessNeighbor(num3, Rot4.East))
			{
				this.rightEdgeOpenIDs[rootZ] = this.newReg.id;
			}
			else if (num4 >= this.mapWidth)
			{
				this.newReg.touchesMapEdge = true;
			}
			Block_14:
			num4--;
			FloodFillRange item = new FloodFillRange(num2, num4, rootZ);
			this.newClosedRanges.Enqueue(item);
		}

		private bool CheckRegionableAndProcessNeighbor(int index, Rot4 processingDirection)
		{
			return this.CheckRegionableAndProcessNeighbor(this.map.cellIndices.IndexToCell(index), processingDirection);
		}

		private bool CheckRegionableAndProcessNeighbor(IntVec3 c, Rot4 processingDirection)
		{
			if (!c.InBounds(this.map))
			{
				this.newReg.touchesMapEdge = true;
				return false;
			}
			if (!c.Walkable(this.map))
			{
				return false;
			}
			Thing regionBarrier = c.GetRegionBarrier(this.map);
			if (regionBarrier != null)
			{
				if (regionBarrier.def.IsDoor)
				{
					this.TryMakePortalSpan(c, processingDirection);
				}
				return false;
			}
			return true;
		}

		private void TryMakePortalSpan(IntVec3 c, Rot4 processingDirection)
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
			this.FinalizeSpanIfValid(ref edgeSpan);
		}

		private bool IndexWasProcessed(int index)
		{
			return this.processedIndices[index] == this.newReg.id;
		}

		private void ResolveSpanLinks()
		{
			EdgeSpan edgeSpan = default(EdgeSpan);
			EdgeSpan edgeSpan2 = default(EdgeSpan);
			for (int i = this.newReg.extentsLimit.minZ; i <= this.newReg.extentsLimit.maxZ; i++)
			{
				if (this.leftEdgeOpenIDs[i] == this.newReg.id)
				{
					this.MakeOrExpandSpan(ref edgeSpan, SpanDirection.North, this.newReg.extentsLimit.minX, i);
				}
				else
				{
					this.FinalizeSpanIfValid(ref edgeSpan);
				}
				if (this.rightEdgeOpenIDs[i] == this.newReg.id)
				{
					this.MakeOrExpandSpan(ref edgeSpan2, SpanDirection.North, this.newReg.extentsLimit.maxX + 1, i);
				}
				else
				{
					this.FinalizeSpanIfValid(ref edgeSpan2);
				}
			}
			this.FinalizeSpanIfValid(ref edgeSpan);
			this.FinalizeSpanIfValid(ref edgeSpan2);
			EdgeSpan edgeSpan3 = default(EdgeSpan);
			EdgeSpan edgeSpan4 = default(EdgeSpan);
			for (int j = this.newReg.extentsLimit.minX; j <= this.newReg.extentsLimit.maxX; j++)
			{
				if (this.bottomEdgeOpenIDs[j] == this.newReg.id)
				{
					this.MakeOrExpandSpan(ref edgeSpan3, SpanDirection.East, j, this.newReg.extentsLimit.minZ);
				}
				else
				{
					this.FinalizeSpanIfValid(ref edgeSpan3);
				}
				if (this.topEdgeOpenIDs[j] == this.newReg.id)
				{
					this.MakeOrExpandSpan(ref edgeSpan4, SpanDirection.East, j, this.newReg.extentsLimit.maxZ + 1);
				}
				else
				{
					this.FinalizeSpanIfValid(ref edgeSpan4);
				}
			}
			this.FinalizeSpanIfValid(ref edgeSpan3);
			this.FinalizeSpanIfValid(ref edgeSpan4);
		}

		private void MakeOrExpandSpan(ref EdgeSpan span, SpanDirection dir, int x, int z)
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

		private void FinalizeSpanIfValid(ref EdgeSpan span)
		{
			if (span.IsValid)
			{
				RegionLink regionLink = this.map.regionLinkDatabase.LinkFrom(span);
				regionLink.Register(this.newReg);
				this.newReg.links.Add(regionLink);
				span = default(EdgeSpan);
			}
		}
	}
}
