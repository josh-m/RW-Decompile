using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;

namespace Verse.AI
{
	public sealed class PathGrid
	{
		private const int ImpassableCost = 10000;

		private Map map;

		public int[] pathGrid;

		public PathGrid(Map map)
		{
			this.map = map;
			this.ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			this.pathGrid = new int[this.map.cellIndices.NumGridCells];
		}

		public bool Walkable(IntVec3 loc)
		{
			return loc.InBounds(this.map) && this.pathGrid[this.map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(IntVec3 loc)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(int x, int z)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(x, z)] < 10000;
		}

		public int PerceivedPathCostAt(IntVec3 loc)
		{
			return this.pathGrid[this.map.cellIndices.CellToIndex(loc)];
		}

		public void RecalculatePerceivedPathCostUnderThing(Thing t)
		{
			if (t.def.size == IntVec2.One)
			{
				this.RecalculatePerceivedPathCostAt(t.Position);
			}
			else
			{
				CellRect cellRect = t.OccupiedRect();
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						IntVec3 c = new IntVec3(j, 0, i);
						this.RecalculatePerceivedPathCostAt(c);
					}
				}
			}
		}

		public void RecalculatePerceivedPathCostAt(IntVec3 c)
		{
			if (!c.InBounds(this.map))
			{
				return;
			}
			bool flag = this.WalkableFast(c);
			this.pathGrid[this.map.cellIndices.CellToIndex(c)] = this.CalculatedCostAt(c, true, IntVec3.Invalid);
			if (this.WalkableFast(c) != flag)
			{
				this.map.regionDirtyer.Notify_WalkabilityChanged(c);
			}
		}

		public void RecalculateAllPerceivedPathCosts()
		{
			foreach (IntVec3 current in this.map.AllCells)
			{
				this.RecalculatePerceivedPathCostAt(current);
			}
		}

		public int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell)
		{
			int num = 0;
			TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(c);
			if (terrainDef == null || terrainDef.passability == Traversability.Impassable)
			{
				num = 10000;
			}
			else
			{
				num += terrainDef.pathCost;
			}
			int num2 = SnowUtility.MovementTicksAddOn(this.map.snowGrid.GetCategory(c));
			num += num2;
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.passability == Traversability.Impassable)
				{
					return 10000;
				}
				if (!PathGrid.IsPathCostIgnoreRepeater(thing.def) || !prevCell.IsValid || !this.ContainsPathCostIgnoreRepeater(prevCell))
				{
					num += thing.def.pathCost;
				}
			}
			if (perceivedStatic)
			{
				for (int j = 0; j < 9; j++)
				{
					IntVec3 b = GenAdj.AdjacentCellsAndInside[j];
					IntVec3 c2 = c + b;
					if (c2.InBounds(this.map))
					{
						Fire fire = null;
						list = this.map.thingGrid.ThingsListAtFast(c2);
						for (int k = 0; k < list.Count; k++)
						{
							fire = (list[k] as Fire);
							if (fire != null)
							{
								break;
							}
						}
						if (fire != null && fire.parent == null)
						{
							if (b.x == 0 && b.z == 0)
							{
								num += 1000;
							}
							else
							{
								num += 150;
							}
						}
					}
				}
			}
			return num;
		}

		private bool ContainsPathCostIgnoreRepeater(IntVec3 c)
		{
			List<Thing> list = this.map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (PathGrid.IsPathCostIgnoreRepeater(list[i].def))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsPathCostIgnoreRepeater(ThingDef def)
		{
			return def.pathCost >= 25 && def.pathCostIgnoreRepeat;
		}

		public static void LogPathCostIgnoreRepeaters()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("===============PATH COST IGNORE REPEATERS==============");
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (PathGrid.IsPathCostIgnoreRepeater(current) && current.passability != Traversability.Impassable)
				{
					stringBuilder.AppendLine(current.defName + " " + current.pathCost);
				}
			}
			stringBuilder.AppendLine("===============NON-PATH COST IGNORE REPEATERS that are buildings with >0 pathCost ==============");
			foreach (ThingDef current2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (!PathGrid.IsPathCostIgnoreRepeater(current2) && current2.passability != Traversability.Impassable && current2.category == ThingCategory.Building && current2.pathCost > 0)
				{
					stringBuilder.AppendLine(current2.defName + " " + current2.pathCost);
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
