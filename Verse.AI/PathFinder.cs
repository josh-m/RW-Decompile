using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse.AI
{
	public class PathFinder
	{
		internal struct PathFinderNodeFast
		{
			public int knownCost;

			public int totalCostEstimate;

			public ushort parentX;

			public ushort parentZ;

			public ushort status;
		}

		private struct PathFinderNode
		{
			public IntVec3 position;

			public IntVec3 parentPosition;
		}

		internal class PathFinderNodeFastCostComparer : IComparer<int>
		{
			private PathFinder.PathFinderNodeFast[] grid;

			public PathFinderNodeFastCostComparer(PathFinder.PathFinderNodeFast[] grid)
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

		public const int DefaultMoveTicksCardinal = 13;

		private const int DefaultMoveTicksDiagonal = 18;

		private const int SearchLimit = 160000;

		private const int Cost_DoorToBash = 300;

		private const int Cost_BlockedWall = 60;

		private const float Cost_BlockedWallPerHitPoint = 0.1f;

		private const int Cost_OutsideAllowedArea = 600;

		private const int Cost_PawnCollision = 800;

		private const int HeuristicStrengthAnimal = 30;

		private Map map;

		private FastPriorityQueue<int> openList;

		private PathFinder.PathFinderNodeFast[] calcGrid;

		private ushort statusOpenValue = 1;

		private ushort statusClosedValue = 2;

		private int mapSizePowTwo;

		private ushort gridSizeX;

		private ushort gridSizeZ;

		private ushort gridSizeXMinus1;

		private ushort gridSizeZLog2;

		private int mapSizeX;

		private int mapSizeZ;

		private PathGrid pathGrid;

		private int[] pathGridDirect;

		private Building[] edificeGrid;

		private PawnPath newPath;

		private int moveTicksCardinal;

		private int moveTicksDiagonal;

		private int curIndex;

		private ushort curX;

		private ushort curZ;

		private IntVec3 curIntVec3 = default(IntVec3);

		private int neighIndex;

		private ushort neighX;

		private ushort neighZ;

		private int neighCostThroughCur;

		private int neighCost;

		private int h;

		private int closedCellCount;

		private int destinationIndex;

		private int destinationX = -1;

		private int destinationZ = -1;

		private CellRect destinationRect;

		private bool destinationIsOneCell;

		private int heuristicStrength;

		private bool debug_pathFailMessaged;

		private int debug_totalOpenListCount;

		private int debug_openCellsPopped;

		private static readonly sbyte[] Directions = new sbyte[]
		{
			0,
			1,
			0,
			-1,
			1,
			1,
			-1,
			-1,
			-1,
			0,
			1,
			0,
			-1,
			1,
			1,
			-1
		};

		private static readonly SimpleCurve HeuristicStrengthHuman_DistanceCurve = new SimpleCurve
		{
			new CurvePoint(40f, 10f),
			new CurvePoint(130f, 35f)
		};

		public PathFinder(Map map)
		{
			this.map = map;
			this.mapSizePowTwo = map.info.PowerOfTwoOverMapSize;
			this.gridSizeX = (ushort)this.mapSizePowTwo;
			this.gridSizeZ = (ushort)this.mapSizePowTwo;
			this.gridSizeXMinus1 = this.gridSizeX - 1;
			this.gridSizeZLog2 = (ushort)Math.Log((double)this.gridSizeZ, 2.0);
			this.mapSizeX = map.Size.x;
			this.mapSizeZ = map.Size.z;
			this.calcGrid = new PathFinder.PathFinderNodeFast[(int)(this.gridSizeX * this.gridSizeZ)];
			this.openList = new FastPriorityQueue<int>(new PathFinder.PathFinderNodeFastCostComparer(this.calcGrid));
		}

		public PawnPath FindPath(IntVec3 start, LocalTargetInfo dest, Pawn pawn, PathEndMode peMode = PathEndMode.OnCell)
		{
			bool flag = false;
			if (pawn != null && pawn.CurJob != null && pawn.CurJob.canBash)
			{
				flag = true;
			}
			bool canBash = flag;
			return this.FindPath(start, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBash), peMode);
		}

		public PawnPath FindPath(IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms, PathEndMode peMode = PathEndMode.OnCell)
		{
			if (DebugSettings.pathThroughWalls)
			{
				traverseParms.mode = TraverseMode.PassAnything;
			}
			Pawn pawn = traverseParms.pawn;
			bool flag = traverseParms.mode == TraverseMode.PassAnything;
			bool flag2 = !flag;
			if (pawn != null && pawn.Map != this.map)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to FindPath for pawn which is spawned in another map. His map PathFinder should have been used, not this one. pawn=",
					pawn,
					" pawn.Map=",
					pawn.Map,
					" map=",
					this.map
				}));
				return PawnPath.NotFound;
			}
			if (!start.IsValid)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to FindPath with invalid start ",
					start,
					", pawn= ",
					pawn
				}));
				return PawnPath.NotFound;
			}
			if (!dest.IsValid)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to FindPath with invalid dest ",
					dest,
					", pawn= ",
					pawn
				}));
				return PawnPath.NotFound;
			}
			if (!flag)
			{
				if (traverseParms.mode == TraverseMode.ByPawn)
				{
					if (!pawn.CanReach(dest, peMode, Danger.Deadly, traverseParms.canBash, traverseParms.mode))
					{
						return PawnPath.NotFound;
					}
				}
				else if (!this.map.reachability.CanReach(start, dest, peMode, traverseParms))
				{
					return PawnPath.NotFound;
				}
			}
			else if (dest.HasThing && dest.Thing.Map != this.map)
			{
				return PawnPath.NotFound;
			}
			ByteGrid byteGrid = (pawn == null) ? null : pawn.GetAvoidGrid();
			this.PfProfilerBeginSample(string.Concat(new object[]
			{
				"FindPath for ",
				pawn,
				" from ",
				start,
				" to ",
				dest,
				(!dest.HasThing) ? string.Empty : (" at " + dest.Cell)
			}));
			this.destinationX = dest.Cell.x;
			this.destinationZ = dest.Cell.z;
			CellIndices cellIndices = this.map.cellIndices;
			this.curIndex = cellIndices.CellToIndex(start);
			this.destinationIndex = cellIndices.CellToIndex(dest.Cell);
			if (!dest.HasThing || peMode == PathEndMode.OnCell)
			{
				this.destinationRect = CellRect.SingleCell(dest.Cell);
			}
			else
			{
				this.destinationRect = dest.Thing.OccupiedRect();
			}
			if (peMode == PathEndMode.Touch)
			{
				this.destinationRect = this.destinationRect.ExpandedBy(1);
			}
			this.destinationIsOneCell = (this.destinationRect.Width == 1 && this.destinationRect.Height == 1);
			this.pathGrid = this.map.pathGrid;
			this.pathGridDirect = this.map.pathGrid.pathGrid;
			EdificeGrid edificeGrid = this.map.edificeGrid;
			this.edificeGrid = edificeGrid.InnerArray;
			this.statusOpenValue += 2;
			this.statusClosedValue += 2;
			if (this.statusClosedValue >= 65435)
			{
				this.ResetStatuses();
			}
			if (pawn != null && pawn.RaceProps.Animal)
			{
				this.heuristicStrength = 30;
			}
			else
			{
				float lengthHorizontal = (start - dest.Cell).LengthHorizontal;
				this.heuristicStrength = Mathf.RoundToInt(PathFinder.HeuristicStrengthHuman_DistanceCurve.Evaluate(lengthHorizontal));
			}
			this.closedCellCount = 0;
			this.openList.Clear();
			this.debug_pathFailMessaged = false;
			this.debug_totalOpenListCount = 0;
			this.debug_openCellsPopped = 0;
			if (pawn != null)
			{
				this.moveTicksCardinal = pawn.TicksPerMoveCardinal;
				this.moveTicksDiagonal = pawn.TicksPerMoveDiagonal;
			}
			else
			{
				this.moveTicksCardinal = 13;
				this.moveTicksDiagonal = 18;
			}
			this.calcGrid[this.curIndex].knownCost = 0;
			this.calcGrid[this.curIndex].totalCostEstimate = 0;
			this.calcGrid[this.curIndex].parentX = (ushort)start.x;
			this.calcGrid[this.curIndex].parentZ = (ushort)start.z;
			this.calcGrid[this.curIndex].status = this.statusOpenValue;
			this.openList.Push(this.curIndex);
			Area area = null;
			if (pawn != null && pawn.playerSettings != null && !pawn.Drafted)
			{
				area = pawn.playerSettings.AreaRestrictionInPawnCurrentMap;
			}
			bool flag3 = false;
			if (pawn != null)
			{
				flag3 = PawnUtility.ShouldCollideWithPawns(pawn);
			}
			while (true)
			{
				this.PfProfilerBeginSample("Open cell");
				if (this.openList.Count <= 0)
				{
					break;
				}
				this.debug_totalOpenListCount += this.openList.Count;
				this.debug_openCellsPopped++;
				this.curIndex = this.openList.Pop();
				if (this.calcGrid[this.curIndex].status == this.statusClosedValue)
				{
					this.PfProfilerEndSample();
				}
				else
				{
					this.curIntVec3 = cellIndices.IndexToCell(this.curIndex);
					this.curX = (ushort)this.curIntVec3.x;
					this.curZ = (ushort)this.curIntVec3.z;
					if (DebugViewSettings.drawPaths)
					{
						this.DebugFlash(this.curIntVec3, (float)this.calcGrid[this.curIndex].knownCost / 1500f, this.calcGrid[this.curIndex].knownCost.ToString());
					}
					if (this.destinationIsOneCell)
					{
						if (this.curIndex == this.destinationIndex)
						{
							goto Block_34;
						}
					}
					else if (this.destinationRect.Contains(this.curIntVec3))
					{
						goto Block_35;
					}
					if (this.closedCellCount > 160000)
					{
						goto Block_36;
					}
					this.PfProfilerEndSample();
					this.PfProfilerBeginSample("Neighbor consideration");
					for (int i = 0; i < 8; i++)
					{
						this.neighX = (ushort)((int)this.curX + (int)PathFinder.Directions[i]);
						this.neighZ = (ushort)((int)this.curZ + (int)PathFinder.Directions[i + 8]);
						IntVec3 intVec = new IntVec3((int)this.neighX, 0, (int)this.neighZ);
						this.neighIndex = cellIndices.CellToIndex((int)this.neighX, (int)this.neighZ);
						if ((int)this.neighX >= this.mapSizeX || (int)this.neighZ >= this.mapSizeZ)
						{
							this.DebugFlash(intVec, 0.75f, "oob");
						}
						else if (this.calcGrid[this.neighIndex].status != this.statusClosedValue)
						{
							int num = 0;
							bool flag4 = false;
							if (!this.pathGrid.WalkableFast(intVec))
							{
								if (!flag)
								{
									this.DebugFlash(intVec, 0.22f, "walk");
									goto IL_FB2;
								}
								flag4 = true;
								num += 60;
								Building building = edificeGrid[intVec];
								if (building == null)
								{
									goto IL_FB2;
								}
								if (!building.def.useHitPoints || !building.def.destroyable)
								{
									goto IL_FB2;
								}
								num += (int)((float)building.HitPoints * 0.1f);
							}
							if (i > 3)
							{
								switch (i)
								{
								case 4:
									if (!this.pathGrid.WalkableFast((int)this.curX, (int)(this.curZ - 1)))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)this.curX, 0, (int)(this.curZ - 1)), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									if (!this.pathGrid.WalkableFast((int)(this.curX + 1), (int)this.curZ))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)(this.curX + 1), 0, (int)this.curZ), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									break;
								case 5:
									if (!this.pathGrid.WalkableFast((int)this.curX, (int)(this.curZ + 1)))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)this.curX, 0, (int)(this.curZ + 1)), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									if (!this.pathGrid.WalkableFast((int)(this.curX + 1), (int)this.curZ))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)(this.curX + 1), 0, (int)this.curZ), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									break;
								case 6:
									if (!this.pathGrid.WalkableFast((int)this.curX, (int)(this.curZ + 1)))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)this.curX, 0, (int)(this.curZ + 1)), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									if (!this.pathGrid.WalkableFast((int)(this.curX - 1), (int)this.curZ))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)(this.curX - 1), 0, (int)this.curZ), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									break;
								case 7:
									if (!this.pathGrid.WalkableFast((int)this.curX, (int)(this.curZ - 1)))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)this.curX, 0, (int)(this.curZ - 1)), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									if (!this.pathGrid.WalkableFast((int)(this.curX - 1), (int)this.curZ))
									{
										if (!flag || !flag2)
										{
											this.DebugFlash(new IntVec3((int)(this.curX - 1), 0, (int)this.curZ), 0.9f, "corn");
											goto IL_FB2;
										}
										num += 60;
									}
									break;
								}
							}
							if (i > 3)
							{
								this.neighCost = this.moveTicksDiagonal;
							}
							else
							{
								this.neighCost = this.moveTicksCardinal;
							}
							this.neighCost += num;
							if (!flag4)
							{
								this.neighCost += this.pathGridDirect[this.neighIndex];
							}
							if (byteGrid != null)
							{
								this.neighCost += (int)(byteGrid[this.neighIndex] * 8);
							}
							if (area != null && !area[intVec])
							{
								this.neighCost += 600;
							}
							if (flag3 && PawnUtility.AnyPawnBlockingPathAt(intVec, pawn, false, false))
							{
								this.neighCost += 800;
							}
							Building building2 = this.edificeGrid[cellIndices.CellToIndex((int)this.neighX, (int)this.neighZ)];
							if (building2 != null)
							{
								this.PfProfilerBeginSample("Edifices");
								Building_Door building_Door = building2 as Building_Door;
								if (building_Door != null)
								{
									switch (traverseParms.mode)
									{
									case TraverseMode.ByPawn:
										if (!traverseParms.canBash && building_Door.IsForbiddenToPass(pawn))
										{
											if (DebugViewSettings.drawPaths)
											{
												this.DebugFlash(building2.Position, 0.77f, "forbid");
											}
											this.PfProfilerEndSample();
											goto IL_FB2;
										}
										if (!building_Door.FreePassage)
										{
											if (building_Door.PawnCanOpen(pawn))
											{
												this.neighCost += building_Door.TicksToOpenNow;
											}
											else
											{
												if (!traverseParms.canBash)
												{
													if (DebugViewSettings.drawPaths)
													{
														this.DebugFlash(building2.Position, 0.34f, "cant pass");
													}
													this.PfProfilerEndSample();
													goto IL_FB2;
												}
												this.neighCost += 300;
											}
										}
										break;
									case TraverseMode.NoPassClosedDoors:
										if (!building_Door.FreePassage)
										{
											this.PfProfilerEndSample();
											goto IL_FB2;
										}
										break;
									}
								}
								else if (pawn != null)
								{
									this.neighCost += (int)building2.PathFindCostFor(pawn);
								}
								this.PfProfilerEndSample();
							}
							this.neighCostThroughCur = this.neighCost + this.calcGrid[this.curIndex].knownCost;
							if ((this.calcGrid[this.neighIndex].status != this.statusClosedValue && this.calcGrid[this.neighIndex].status != this.statusOpenValue) || this.calcGrid[this.neighIndex].knownCost > this.neighCostThroughCur)
							{
								int status = (int)this.calcGrid[this.neighIndex].status;
								this.calcGrid[this.neighIndex].parentX = this.curX;
								this.calcGrid[this.neighIndex].parentZ = this.curZ;
								this.calcGrid[this.neighIndex].knownCost = this.neighCostThroughCur;
								this.calcGrid[this.neighIndex].status = this.statusOpenValue;
								this.h = this.heuristicStrength * (Mathf.Abs((int)this.neighX - this.destinationX) + Mathf.Abs((int)this.neighZ - this.destinationZ));
								this.calcGrid[this.neighIndex].totalCostEstimate = this.neighCostThroughCur + this.h;
								if (status != (int)this.statusOpenValue)
								{
									this.openList.Push(this.neighIndex);
								}
							}
						}
						IL_FB2:;
					}
					this.PfProfilerEndSample();
					this.closedCellCount++;
					this.calcGrid[this.curIndex].status = this.statusClosedValue;
				}
			}
			if (!this.debug_pathFailMessaged)
			{
				string text = (pawn == null || pawn.CurJob == null) ? "null" : pawn.CurJob.ToString();
				string text2 = (pawn == null || pawn.Faction == null) ? "null" : pawn.Faction.ToString();
				Log.Warning(string.Concat(new object[]
				{
					pawn,
					" pathing from ",
					start,
					" to ",
					dest,
					" ran out of cells to process.\nJob:",
					text,
					"\nFaction: ",
					text2,
					"\n\nThis will be the last message to avoid spam."
				}));
				this.debug_pathFailMessaged = true;
			}
			this.DebugDrawRichData();
			this.PfProfilerEndSample();
			return PawnPath.NotFound;
			Block_34:
			this.PfProfilerEndSample();
			return this.FinalizedPath();
			Block_35:
			this.PfProfilerEndSample();
			return this.FinalizedPath();
			Block_36:
			Log.Warning(string.Concat(new object[]
			{
				pawn,
				" pathing from ",
				start,
				" to ",
				dest,
				" hit search limit of ",
				160000,
				" cells."
			}));
			this.DebugDrawRichData();
			this.PfProfilerEndSample();
			return PawnPath.NotFound;
		}

		private void DebugFlash(IntVec3 c, float colorPct, string str)
		{
			if (DebugViewSettings.drawPaths)
			{
				this.map.debugDrawer.FlashCell(c, colorPct, str);
			}
		}

		private PawnPath FinalizedPath()
		{
			this.newPath = this.map.pawnPathPool.GetEmptyPawnPath();
			IntVec3 parentPosition = new IntVec3((int)this.curX, 0, (int)this.curZ);
			CellIndices cellIndices = this.map.cellIndices;
			while (true)
			{
				PathFinder.PathFinderNodeFast pathFinderNodeFast = this.calcGrid[cellIndices.CellToIndex(parentPosition)];
				PathFinder.PathFinderNode pathFinderNode;
				pathFinderNode.parentPosition = new IntVec3((int)pathFinderNodeFast.parentX, 0, (int)pathFinderNodeFast.parentZ);
				pathFinderNode.position = parentPosition;
				this.newPath.AddNode(pathFinderNode.position);
				if (pathFinderNode.position == pathFinderNode.parentPosition)
				{
					break;
				}
				parentPosition = pathFinderNode.parentPosition;
			}
			this.newPath.SetupFound((float)this.calcGrid[this.curIndex].knownCost);
			this.PfProfilerEndSample();
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

		[Conditional("PFPROFILE")]
		private void PfProfilerBeginSample(string s)
		{
		}

		[Conditional("PFPROFILE")]
		private void PfProfilerEndSample()
		{
		}

		private void DebugDrawRichData()
		{
			if (DebugViewSettings.drawPaths)
			{
				while (this.openList.Count > 0)
				{
					int num = this.openList.Pop();
					IntVec3 c = new IntVec3(num & (int)this.gridSizeXMinus1, 0, num >> (int)this.gridSizeZLog2);
					this.map.debugDrawer.FlashCell(c, 0f, "open");
				}
			}
		}
	}
}
