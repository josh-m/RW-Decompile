using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathGrid
	{
		private const int ImpassableCost = 1000000;

		public int[] pathGrid;

		private int allPathCostsRecalculatedDayOfYear = -1;

		private static int DayOfYearAt0Long
		{
			get
			{
				return GenDate.DayOfYear((long)GenTicks.TicksAbs, 0f);
			}
		}

		public WorldPathGrid()
		{
			this.ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			this.pathGrid = new int[Find.WorldGrid.TilesCount];
		}

		public void WorldPathGridTick()
		{
			if (this.allPathCostsRecalculatedDayOfYear != WorldPathGrid.DayOfYearAt0Long)
			{
				this.RecalculateAllPerceivedPathCosts();
			}
		}

		public bool Passable(int tile)
		{
			return Find.WorldGrid.InBounds(tile) && this.pathGrid[tile] < 1000000;
		}

		public bool PassableFast(int tile)
		{
			return this.pathGrid[tile] < 1000000;
		}

		public int PerceivedPathCostAt(int tile)
		{
			return this.pathGrid[tile];
		}

		public void RecalculatePerceivedPathCostAt(int tile)
		{
			if (!Find.WorldGrid.InBounds(tile))
			{
				return;
			}
			bool flag = this.PassableFast(tile);
			this.pathGrid[tile] = WorldPathGrid.CalculatedCostAt(tile, true, -1f);
			if (flag != this.PassableFast(tile))
			{
				Find.WorldReachability.ClearCache();
			}
		}

		public void RecalculateAllPerceivedPathCosts()
		{
			this.allPathCostsRecalculatedDayOfYear = WorldPathGrid.DayOfYearAt0Long;
			for (int i = 0; i < this.pathGrid.Length; i++)
			{
				this.RecalculatePerceivedPathCostAt(i);
			}
		}

		public static int CalculatedCostAt(int tile, bool perceivedStatic, float yearPercent = -1f)
		{
			int num = 0;
			Tile tile2 = Find.WorldGrid[tile];
			if (tile2.biome.impassable)
			{
				return 1000000;
			}
			if (yearPercent < 0f)
			{
				yearPercent = (float)WorldPathGrid.DayOfYearAt0Long / 60f;
			}
			num += Mathf.RoundToInt(tile2.biome.pathCost.Evaluate(yearPercent));
			if (tile2.hilliness == Hilliness.Impassable)
			{
				return 1000000;
			}
			num += WorldPathGrid.CostFromTileHilliness(tile2.hilliness);
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			int i = 0;
			int count = allWorldObjects.Count;
			while (i < count)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (worldObject.Tile == tile)
				{
					if (worldObject.def.impassable)
					{
						return 1000000;
					}
					num += worldObject.def.pathCost;
				}
				i++;
			}
			return num;
		}

		private static int CostFromTileHilliness(Hilliness hilliness)
		{
			switch (hilliness)
			{
			case Hilliness.Flat:
				return 0;
			case Hilliness.SmallHills:
				return 2000;
			case Hilliness.LargeHills:
				return 6000;
			case Hilliness.Mountainous:
				return 30000;
			case Hilliness.Impassable:
				return 30000;
			default:
				return 0;
			}
		}
	}
}
