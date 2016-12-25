using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public sealed class Region
	{
		public const int GridSize = 12;

		public int id = -1;

		private Room roomInt;

		public List<RegionLink> links = new List<RegionLink>();

		public CellRect extentsClose;

		public CellRect extentsLimit;

		public Building_Door portal;

		private int precalculatedHashCode;

		public bool touchesMapEdge;

		private int cachedCellCount = -1;

		public bool valid = true;

		private ListerThings listerThings = new ListerThings(ListerThingsUse.Region);

		public uint[] closedIndex = new uint[RegionTraverser.NumWorkers];

		public uint reachedIndex;

		public int newRegionGroupIndex = -1;

		private Dictionary<Area, AreaOverlap> cachedAreaOverlaps;

		public int mark;

		private int debug_makeTick = -1000;

		private int debug_lastTraverseTick = -1000;

		private static int nextId = 1;

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				RegionGrid regions = Find.RegionGrid;
				for (int z = this.extentsClose.minZ; z <= this.extentsClose.maxZ; z++)
				{
					for (int x = this.extentsClose.minX; x <= this.extentsClose.maxX; x++)
					{
						IntVec3 c = new IntVec3(x, 0, z);
						if (regions.GetRegionAt_InvalidAllowed(c) == this)
						{
							yield return c;
						}
					}
				}
			}
		}

		public int CellCount
		{
			get
			{
				if (this.cachedCellCount == -1)
				{
					this.cachedCellCount = this.Cells.Count<IntVec3>();
				}
				return this.cachedCellCount;
			}
		}

		public IEnumerable<Region> Neighbors
		{
			get
			{
				for (int li = 0; li < this.links.Count; li++)
				{
					RegionLink link = this.links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].valid)
						{
							yield return link.regions[ri];
						}
					}
				}
			}
		}

		public IEnumerable<Region> NonPortalNeighbors
		{
			get
			{
				for (int li = 0; li < this.links.Count; li++)
				{
					RegionLink link = this.links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].portal == null && link.regions[ri].valid)
						{
							yield return link.regions[ri];
						}
					}
				}
			}
		}

		public Room Room
		{
			get
			{
				return this.roomInt;
			}
			set
			{
				if (value == this.roomInt)
				{
					return;
				}
				if (this.roomInt != null)
				{
					this.roomInt.RemoveRegion(this);
				}
				this.roomInt = value;
				if (this.roomInt != null)
				{
					this.roomInt.AddRegion(this);
				}
			}
		}

		public IntVec3 RandomCell
		{
			get
			{
				for (int i = 0; i < 1000; i++)
				{
					IntVec3 randomCell = this.extentsClose.RandomCell;
					if (randomCell.GetRegion() == this)
					{
						return randomCell;
					}
				}
				Log.Error("Couldn't find random cell in region " + this.ToString());
				return this.extentsClose.RandomCell;
			}
		}

		public string DebugString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("id: " + this.id);
				stringBuilder.AppendLine("links count: " + this.links.Count);
				foreach (RegionLink current in this.links)
				{
					stringBuilder.AppendLine("  --" + current.ToString());
				}
				stringBuilder.AppendLine("valid: " + this.valid.ToString());
				stringBuilder.AppendLine("makeTick: " + this.debug_makeTick);
				stringBuilder.AppendLine("roomID: " + ((this.Room == null) ? "null room!" : this.Room.ID.ToString()));
				stringBuilder.AppendLine("extentsClose: " + this.extentsClose);
				stringBuilder.AppendLine("extentsLimit: " + this.extentsLimit);
				stringBuilder.AppendLine("ListerThings:");
				if (this.listerThings.AllThings != null)
				{
					for (int i = 0; i < this.listerThings.AllThings.Count; i++)
					{
						stringBuilder.AppendLine("  --" + this.listerThings.AllThings[i]);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public bool DebugIsNew
		{
			get
			{
				return this.debug_makeTick > Find.TickManager.TicksGame - 60;
			}
		}

		public ListerThings ListerThings
		{
			get
			{
				return this.listerThings;
			}
		}

		private Region()
		{
		}

		public static Region MakeNewUnfilled(IntVec3 root)
		{
			Region region = new Region();
			region.debug_makeTick = Find.TickManager.TicksGame;
			region.id = Region.nextId;
			Region.nextId++;
			region.precalculatedHashCode = Gen.HashCombineInt(region.id, 1295813358);
			region.extentsClose.minX = root.x;
			region.extentsClose.maxX = root.x;
			region.extentsClose.minZ = root.z;
			region.extentsClose.maxZ = root.z;
			region.extentsLimit.minX = root.x - root.x % 12;
			region.extentsLimit.maxX = root.x + 12 - (root.x + 12) % 12 - 1;
			region.extentsLimit.minZ = root.z - root.z % 12;
			region.extentsLimit.maxZ = root.z + 12 - (root.z + 12) % 12 - 1;
			region.extentsLimit.ClipInsideMap();
			return region;
		}

		public bool Allows(TraverseParms tp, bool isDestination)
		{
			if (tp.maxDanger < Danger.Deadly && tp.pawn != null)
			{
				Danger danger = this.DangerFor(tp.pawn);
				if ((isDestination || danger == Danger.Deadly) && danger > tp.maxDanger)
				{
					return false;
				}
			}
			switch (tp.mode)
			{
			case TraverseMode.ByPawn:
			{
				if (this.portal == null)
				{
					return true;
				}
				ByteGrid avoidGrid = tp.pawn.GetAvoidGrid();
				if (avoidGrid != null && avoidGrid[this.portal.Position] == 255)
				{
					return false;
				}
				if (tp.pawn.HostileTo(this.portal))
				{
					return this.portal.CanPhysicallyPass(tp.pawn) || tp.canBash;
				}
				return this.portal.CanPhysicallyPass(tp.pawn) && !this.portal.IsForbiddenToPass(tp.pawn);
			}
			case TraverseMode.PassDoors:
				return true;
			case TraverseMode.NoPassClosedDoors:
				return this.portal == null || this.portal.FreePassage;
			default:
				throw new NotImplementedException();
			}
		}

		public Danger DangerFor(Pawn p)
		{
			Room room = this.Room;
			float temperature = room.Temperature;
			FloatRange floatRange = p.SafeTemperatureRange();
			if (floatRange.Includes(temperature))
			{
				return Danger.None;
			}
			if (floatRange.ExpandedBy(80f).Includes(temperature))
			{
				return Danger.Some;
			}
			return Danger.Deadly;
		}

		public AreaOverlap OverlapWith(Area a)
		{
			if (this.cachedAreaOverlaps == null)
			{
				this.cachedAreaOverlaps = new Dictionary<Area, AreaOverlap>();
			}
			AreaOverlap areaOverlap;
			if (!this.cachedAreaOverlaps.TryGetValue(a, out areaOverlap))
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 current in this.Cells)
				{
					num2++;
					if (a[current])
					{
						num++;
					}
				}
				if (num == 0)
				{
					areaOverlap = AreaOverlap.None;
				}
				else if (num == num2)
				{
					areaOverlap = AreaOverlap.Entire;
				}
				else
				{
					areaOverlap = AreaOverlap.Partial;
				}
				this.cachedAreaOverlaps.Add(a, areaOverlap);
			}
			return areaOverlap;
		}

		public void Notify_AreaChanged(Area a)
		{
			if (this.cachedAreaOverlaps == null)
			{
				return;
			}
			if (this.cachedAreaOverlaps.ContainsKey(a))
			{
				this.cachedAreaOverlaps.Remove(a);
			}
		}

		public override string ToString()
		{
			string str;
			if (this.portal != null)
			{
				str = this.portal.ToString();
			}
			else
			{
				str = "null";
			}
			return string.Concat(new object[]
			{
				"Region(id=",
				this.id,
				", center=",
				this.extentsClose.CenterCell,
				", links=",
				this.links.Count,
				", cells=",
				this.CellCount,
				(this.portal == null) ? null : (", portal=" + str),
				")"
			});
		}

		public void DebugDraw()
		{
			if (DebugViewSettings.drawRegionTraversal && Find.TickManager.TicksGame < this.debug_lastTraverseTick + 60)
			{
				float a = 1f - (float)(Find.TickManager.TicksGame - this.debug_lastTraverseTick) / 60f;
				GenDraw.DrawFieldEdges(this.Cells.ToList<IntVec3>(), new Color(0f, 0f, 1f, a));
			}
		}

		public void DebugDrawMouseover()
		{
			int num = Mathf.RoundToInt(Time.realtimeSinceStartup * 2f) % 2;
			if (DebugViewSettings.drawRegions)
			{
				Color color;
				if (this.DebugIsNew)
				{
					color = Color.yellow;
				}
				else if (this.valid)
				{
					color = Color.green;
				}
				else
				{
					color = Color.red;
				}
				GenDraw.DrawFieldEdges(this.Cells.ToList<IntVec3>(), color);
				foreach (Region current in this.Neighbors)
				{
					GenDraw.DrawFieldEdges(current.Cells.ToList<IntVec3>(), Color.grey);
				}
			}
			if (DebugViewSettings.drawRegionLinks)
			{
				foreach (RegionLink current2 in this.links)
				{
					if (num == 1)
					{
						foreach (IntVec3 current3 in current2.span.Cells)
						{
							CellRenderer.RenderCell(current3, DebugSolidColorMats.MaterialOf(Color.magenta));
						}
					}
				}
			}
		}

		public void Debug_Notify_Traversed()
		{
			this.debug_lastTraverseTick = Find.TickManager.TicksGame;
		}

		public override int GetHashCode()
		{
			return this.precalculatedHashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Region region = obj as Region;
			return region != null && region.id == this.id;
		}
	}
}
