using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathFinder
	{
		private struct PathFinderNodeFast
		{
			public int knownCost;

			public int totalCostEstimate;

			public int parentTile;

			public ushort status;
		}

		private class PathFinderNodeFastCostComparer : IComparer<int>
		{
			private WorldPathFinder.PathFinderNodeFast[] grid;

			public PathFinderNodeFastCostComparer(WorldPathFinder.PathFinderNodeFast[] grid)
			{
				this.grid = grid;
			}

			public int Compare(int a, int b)
			{
				if (this.grid[a].totalCostEstimate > this.grid[b].totalCostEstimate)
				{
					return 1;
				}
				if (this.grid[a].totalCostEstimate < this.grid[b].totalCostEstimate)
				{
					return -1;
				}
				return 0;
			}
		}

		public const int DefaultMoveTicks = 2600;

		private const int SearchLimit = 300000;

		private FastPriorityQueue<int> openList;

		private WorldPathFinder.PathFinderNodeFast[] calcGrid;

		private ushort statusOpenValue = 1;

		private ushort statusClosedValue = 2;

		private WorldPath newPath;

		private int moveTicks;

		private int curTile;

		private int neighCostThroughCur;

		private int neighCost;

		private int h;

		private int closedTileCount;

		private int heuristicStrength;

		private static readonly SimpleCurve HeuristicStrength_DistanceCurve = new SimpleCurve
		{
			new CurvePoint(40f, 6500f),
			new CurvePoint(130f, 8000f)
		};

		public WorldPathFinder()
		{
			this.calcGrid = new WorldPathFinder.PathFinderNodeFast[Find.WorldGrid.TilesCount];
			this.openList = new FastPriorityQueue<int>(new WorldPathFinder.PathFinderNodeFastCostComparer(this.calcGrid));
		}

		public WorldPath FindPath(int startTile, int destTile, Caravan caravan)
		{
			if (startTile < 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to FindPath with invalid start tile ",
					startTile,
					", caravan= ",
					caravan
				}));
				return WorldPath.NotFound;
			}
			if (destTile < 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to FindPath with invalid dest tile ",
					destTile,
					", caravan= ",
					caravan
				}));
				return WorldPath.NotFound;
			}
			if (caravan != null)
			{
				if (!caravan.CanReach(destTile))
				{
					return WorldPath.NotFound;
				}
			}
			else if (!Find.WorldReachability.CanReach(startTile, destTile))
			{
				return WorldPath.NotFound;
			}
			this.curTile = startTile;
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<Vector3> verts = grid.verts;
			Vector3 normalized = grid.GetTileCenter(destTile).normalized;
			int[] pathGrid = world.pathGrid.pathGrid;
			this.statusOpenValue += 2;
			this.statusClosedValue += 2;
			if (this.statusClosedValue >= 65435)
			{
				this.ResetStatuses();
			}
			float x = Find.WorldGrid.ApproxDistanceInTiles(startTile, destTile);
			this.heuristicStrength = Mathf.RoundToInt(WorldPathFinder.HeuristicStrength_DistanceCurve.Evaluate(x));
			this.closedTileCount = 0;
			this.openList.Clear();
			if (caravan != null)
			{
				this.moveTicks = caravan.TicksPerMove;
			}
			else
			{
				this.moveTicks = 2600;
			}
			this.calcGrid[this.curTile].knownCost = 0;
			this.calcGrid[this.curTile].totalCostEstimate = 0;
			this.calcGrid[this.curTile].parentTile = startTile;
			this.calcGrid[this.curTile].status = this.statusOpenValue;
			this.openList.Push(this.curTile);
			while (this.openList.Count > 0)
			{
				this.curTile = this.openList.Pop();
				if (this.calcGrid[this.curTile].status != this.statusClosedValue)
				{
					if (DebugViewSettings.drawPaths)
					{
						Find.WorldDebugDrawer.FlashTile(this.curTile, (float)this.calcGrid[this.curTile].knownCost / 375000f, this.calcGrid[this.curTile].knownCost.ToString());
					}
					if (this.curTile == destTile)
					{
						return this.FinalizedPath();
					}
					if (this.closedTileCount > 300000)
					{
						Log.Warning(string.Concat(new object[]
						{
							caravan,
							" pathing from ",
							startTile,
							" to ",
							destTile,
							" hit search limit of ",
							300000,
							" tiles."
						}));
						return WorldPath.NotFound;
					}
					int num = (this.curTile + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[this.curTile + 1];
					for (int i = tileIDToNeighbors_offsets[this.curTile]; i < num; i++)
					{
						int num2 = tileIDToNeighbors_values[i];
						if (this.calcGrid[num2].status != this.statusClosedValue)
						{
							if (!world.Impassable(num2))
							{
								this.neighCost = this.moveTicks;
								this.neighCost += pathGrid[num2];
								this.neighCostThroughCur = this.neighCost + this.calcGrid[this.curTile].knownCost;
								if ((this.calcGrid[num2].status != this.statusClosedValue && this.calcGrid[num2].status != this.statusOpenValue) || this.calcGrid[num2].knownCost > this.neighCostThroughCur)
								{
									Vector3 vector = verts[tileIDToVerts_offsets[num2]];
									int status = (int)this.calcGrid[num2].status;
									this.calcGrid[num2].parentTile = this.curTile;
									this.calcGrid[num2].knownCost = this.neighCostThroughCur;
									this.calcGrid[num2].status = this.statusOpenValue;
									this.h = Mathf.RoundToInt((float)this.heuristicStrength * grid.ApproxDistanceInTiles(GenMath.SphericalDistance(vector.normalized, normalized)));
									this.calcGrid[num2].totalCostEstimate = this.neighCostThroughCur + this.h;
									if (status != (int)this.statusOpenValue)
									{
										this.openList.Push(num2);
									}
								}
							}
						}
					}
					this.closedTileCount++;
					this.calcGrid[this.curTile].status = this.statusClosedValue;
				}
			}
			Log.Warning(string.Concat(new object[]
			{
				caravan,
				" pathing from ",
				startTile,
				" to ",
				destTile,
				" ran out of tiles to process."
			}));
			return WorldPath.NotFound;
		}

		private WorldPath FinalizedPath()
		{
			this.newPath = Find.WorldPathPool.GetEmptyWorldPath();
			int num = this.curTile;
			while (true)
			{
				WorldPathFinder.PathFinderNodeFast pathFinderNodeFast = this.calcGrid[num];
				int parentTile = pathFinderNodeFast.parentTile;
				int num2 = num;
				this.newPath.AddNode(num2);
				if (num2 == parentTile)
				{
					break;
				}
				num = parentTile;
			}
			this.newPath.SetupFound((float)this.calcGrid[this.curTile].knownCost);
			return this.newPath;
		}

		private void ResetStatuses()
		{
			int num = this.calcGrid.Length;
			for (int i = 0; i < num; i++)
			{
				this.calcGrid[i].status = 0;
			}
			this.statusOpenValue = 1;
			this.statusClosedValue = 2;
		}
	}
}
