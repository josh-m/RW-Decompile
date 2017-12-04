using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathFinder
	{
		private struct CostNode
		{
			public int tile;

			public int cost;

			public CostNode(int tile, int cost)
			{
				this.tile = tile;
				this.cost = cost;
			}
		}

		private struct PathFinderNodeFast
		{
			public int knownCost;

			public int heuristicCost;

			public int parentTile;

			public int costNodeCost;

			public ushort status;
		}

		private class CostNodeComparer : IComparer<WorldPathFinder.CostNode>
		{
			public int Compare(WorldPathFinder.CostNode a, WorldPathFinder.CostNode b)
			{
				int cost = a.cost;
				int cost2 = b.cost;
				if (cost > cost2)
				{
					return 1;
				}
				if (cost < cost2)
				{
					return -1;
				}
				return 0;
			}
		}

		private FastPriorityQueue<WorldPathFinder.CostNode> openList;

		private WorldPathFinder.PathFinderNodeFast[] calcGrid;

		private ushort statusOpenValue = 1;

		private ushort statusClosedValue = 2;

		private const int SearchLimit = 500000;

		private static readonly SimpleCurve HeuristicStrength_DistanceCurve = new SimpleCurve
		{
			{
				new CurvePoint(30f, 1f),
				true
			},
			{
				new CurvePoint(40f, 1.3f),
				true
			},
			{
				new CurvePoint(130f, 2f),
				true
			}
		};

		private const float BestRoadDiscount = 0.5f;

		public WorldPathFinder()
		{
			this.calcGrid = new WorldPathFinder.PathFinderNodeFast[Find.WorldGrid.TilesCount];
			this.openList = new FastPriorityQueue<WorldPathFinder.CostNode>(new WorldPathFinder.CostNodeComparer());
		}

		public WorldPath FindPath(int startTile, int destTile, Caravan caravan, Func<float, bool> terminator = null)
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
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			Vector3 normalized = grid.GetTileCenter(destTile).normalized;
			int[] pathGrid = world.pathGrid.pathGrid;
			int num = 0;
			int num2 = (caravan == null) ? 2500 : caravan.TicksPerMove;
			int num3 = this.CalculateHeuristicStrength(startTile, destTile);
			this.statusOpenValue += 2;
			this.statusClosedValue += 2;
			if (this.statusClosedValue >= 65435)
			{
				this.ResetStatuses();
			}
			this.calcGrid[startTile].knownCost = 0;
			this.calcGrid[startTile].heuristicCost = 0;
			this.calcGrid[startTile].costNodeCost = 0;
			this.calcGrid[startTile].parentTile = startTile;
			this.calcGrid[startTile].status = this.statusOpenValue;
			this.openList.Clear();
			this.openList.Push(new WorldPathFinder.CostNode(startTile, 0));
			while (this.openList.Count > 0)
			{
				WorldPathFinder.CostNode costNode = this.openList.Pop();
				if (costNode.cost == this.calcGrid[costNode.tile].costNodeCost)
				{
					int tile = costNode.tile;
					if (this.calcGrid[tile].status != this.statusClosedValue)
					{
						if (DebugViewSettings.drawPaths)
						{
							Find.WorldDebugDrawer.FlashTile(tile, (float)this.calcGrid[tile].knownCost / 375000f, this.calcGrid[tile].knownCost.ToString(), 50);
						}
						if (tile == destTile)
						{
							return this.FinalizedPath(tile);
						}
						if (num > 500000)
						{
							Log.Warning(string.Concat(new object[]
							{
								caravan,
								" pathing from ",
								startTile,
								" to ",
								destTile,
								" hit search limit of ",
								500000,
								" tiles."
							}));
							return WorldPath.NotFound;
						}
						int num4 = (tile + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[tile + 1];
						for (int i = tileIDToNeighbors_offsets[tile]; i < num4; i++)
						{
							int num5 = tileIDToNeighbors_values[i];
							if (this.calcGrid[num5].status != this.statusClosedValue)
							{
								if (!world.Impassable(num5))
								{
									int num6 = num2;
									num6 += pathGrid[num5];
									num6 = (int)((float)num6 * grid.GetRoadMovementMultiplierFast(tile, num5));
									int num7 = num6 + this.calcGrid[tile].knownCost;
									ushort status = this.calcGrid[num5].status;
									if ((status != this.statusClosedValue && status != this.statusOpenValue) || this.calcGrid[num5].knownCost > num7)
									{
										Vector3 tileCenter = grid.GetTileCenter(num5);
										if (status != this.statusClosedValue && status != this.statusOpenValue)
										{
											float num8 = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, normalized));
											this.calcGrid[num5].heuristicCost = Mathf.RoundToInt((float)num2 * num8 * (float)num3 * 0.5f);
										}
										int num9 = num7 + this.calcGrid[num5].heuristicCost;
										this.calcGrid[num5].parentTile = tile;
										this.calcGrid[num5].knownCost = num7;
										this.calcGrid[num5].status = this.statusOpenValue;
										this.calcGrid[num5].costNodeCost = num9;
										this.openList.Push(new WorldPathFinder.CostNode(num5, num9));
									}
								}
							}
						}
						num++;
						this.calcGrid[tile].status = this.statusClosedValue;
						if (terminator != null && terminator((float)this.calcGrid[tile].costNodeCost))
						{
							return WorldPath.NotFound;
						}
					}
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

		public void FloodPathsWithCost(List<int> startTiles, Func<int, int, int> costFunc, Func<int, bool> impassable = null, Func<int, float, bool> terminator = null)
		{
			if (startTiles.Count < 1 || startTiles.Contains(-1))
			{
				Log.Error("Tried to FindPath with invalid start tiles");
				return;
			}
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			if (impassable == null)
			{
				impassable = ((int tid) => world.Impassable(tid));
			}
			this.statusOpenValue += 2;
			this.statusClosedValue += 2;
			if (this.statusClosedValue >= 65435)
			{
				this.ResetStatuses();
			}
			this.openList.Clear();
			foreach (int current in startTiles)
			{
				this.calcGrid[current].knownCost = 0;
				this.calcGrid[current].costNodeCost = 0;
				this.calcGrid[current].parentTile = current;
				this.calcGrid[current].status = this.statusOpenValue;
				this.openList.Push(new WorldPathFinder.CostNode(current, 0));
			}
			while (this.openList.Count > 0)
			{
				WorldPathFinder.CostNode costNode = this.openList.Pop();
				if (costNode.cost == this.calcGrid[costNode.tile].costNodeCost)
				{
					int tile = costNode.tile;
					if (this.calcGrid[tile].status != this.statusClosedValue)
					{
						int num = (tile + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[tile + 1];
						for (int i = tileIDToNeighbors_offsets[tile]; i < num; i++)
						{
							int num2 = tileIDToNeighbors_values[i];
							if (this.calcGrid[num2].status != this.statusClosedValue)
							{
								if (!impassable(num2))
								{
									int num3 = costFunc(tile, num2);
									int num4 = num3 + this.calcGrid[tile].knownCost;
									ushort status = this.calcGrid[num2].status;
									if ((status != this.statusClosedValue && status != this.statusOpenValue) || this.calcGrid[num2].knownCost > num4)
									{
										int num5 = num4;
										this.calcGrid[num2].parentTile = tile;
										this.calcGrid[num2].knownCost = num4;
										this.calcGrid[num2].status = this.statusOpenValue;
										this.calcGrid[num2].costNodeCost = num5;
										this.openList.Push(new WorldPathFinder.CostNode(num2, num5));
									}
								}
							}
						}
						this.calcGrid[tile].status = this.statusClosedValue;
						if (terminator != null && terminator(tile, (float)this.calcGrid[tile].costNodeCost))
						{
							break;
						}
					}
				}
			}
		}

		public List<int>[] FloodPathsWithCostForTree(List<int> startTiles, Func<int, int, int> costFunc, Func<int, bool> impassable = null, Func<int, float, bool> terminator = null)
		{
			this.FloodPathsWithCost(startTiles, costFunc, impassable, terminator);
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int>[] array = new List<int>[grid.TilesCount];
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (this.calcGrid[i].status == this.statusClosedValue)
				{
					int parentTile = this.calcGrid[i].parentTile;
					if (parentTile != i)
					{
						if (array[parentTile] == null)
						{
							array[parentTile] = new List<int>();
						}
						array[parentTile].Add(i);
					}
				}
			}
			return array;
		}

		private WorldPath FinalizedPath(int lastTile)
		{
			WorldPath emptyWorldPath = Find.WorldPathPool.GetEmptyWorldPath();
			int num = lastTile;
			while (true)
			{
				WorldPathFinder.PathFinderNodeFast pathFinderNodeFast = this.calcGrid[num];
				int parentTile = pathFinderNodeFast.parentTile;
				int num2 = num;
				emptyWorldPath.AddNode(num2);
				if (num2 == parentTile)
				{
					break;
				}
				num = parentTile;
			}
			emptyWorldPath.SetupFound((float)this.calcGrid[lastTile].knownCost);
			return emptyWorldPath;
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

		private int CalculateHeuristicStrength(int startTile, int destTile)
		{
			float x = Find.WorldGrid.ApproxDistanceInTiles(startTile, destTile);
			return Mathf.RoundToInt(WorldPathFinder.HeuristicStrength_DistanceCurve.Evaluate(x));
		}

		public static int StandardPathCost(int curTile, int neigh, Caravan caravan)
		{
			int num = (caravan == null) ? 2500 : caravan.TicksPerMove;
			num += Find.World.pathGrid.pathGrid[neigh];
			return (int)((float)num * Find.WorldGrid.GetRoadMovementMultiplierFast(curTile, neigh));
		}
	}
}
