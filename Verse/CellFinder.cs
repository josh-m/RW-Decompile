using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace Verse
{
	public static class CellFinder
	{
		private static List<IntVec3> workingCells = new List<IntVec3>();

		private static List<Region> workingRegions = new List<Region>();

		private static List<int> workingListX = new List<int>();

		private static List<int> workingListZ = new List<int>();

		private static List<IntVec3> mapEdgeCells;

		private static IntVec3 mapEdgeCellsSize;

		private static Dictionary<IntVec3, float> tmpDistances = new Dictionary<IntVec3, float>();

		public static IntVec3 RandomCell()
		{
			return new IntVec3(Rand.Range(0, Find.Map.Size.x), 0, Rand.Range(0, Find.Map.Size.z));
		}

		public static IntVec3 RandomEdgeCell()
		{
			IntVec3 result = default(IntVec3);
			if (Rand.Value < 0.5f)
			{
				if (Rand.Value < 0.5f)
				{
					result.x = 0;
				}
				else
				{
					result.x = Find.Map.Size.x - 1;
				}
				result.z = Rand.Range(0, Find.Map.Size.z);
			}
			else
			{
				if (Rand.Value < 0.5f)
				{
					result.z = 0;
				}
				else
				{
					result.z = Find.Map.Size.z - 1;
				}
				result.x = Rand.Range(0, Find.Map.Size.x);
			}
			return result;
		}

		public static IntVec3 RandomNotEdgeCell(int minEdgeDistance)
		{
			int newX = Rand.Range(minEdgeDistance, Find.Map.Size.x - minEdgeDistance);
			int newZ = Rand.Range(minEdgeDistance, Find.Map.Size.z - minEdgeDistance);
			return new IntVec3(newX, 0, newZ);
		}

		public static bool TryFindClosestRegionWith(Region rootReg, TraverseParms traverseParms, Predicate<Region> validator, int maxRegions, out Region result)
		{
			if (rootReg == null)
			{
				result = null;
				return false;
			}
			Region localResult = null;
			RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region r) => r.Allows(traverseParms, true), delegate(Region r)
			{
				if (validator(r))
				{
					localResult = r;
					return true;
				}
				return false;
			}, maxRegions);
			result = localResult;
			return result != null;
		}

		public static Region RandomRegionNear(Region root, int maxRegions, TraverseParms traverseParms, Predicate<Region> validator = null, Pawn pawnToAllow = null)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root");
			}
			if (maxRegions <= 1)
			{
				return root;
			}
			CellFinder.workingRegions.Clear();
			RegionTraverser.BreadthFirstTraverse(root, (Region from, Region r) => (validator == null || validator(r)) && r.Allows(traverseParms, true) && (pawnToAllow == null || !r.IsForbiddenEntirely(pawnToAllow)), delegate(Region r)
			{
				CellFinder.workingRegions.Add(r);
				return false;
			}, maxRegions);
			return CellFinder.workingRegions.RandomElementByWeight((Region r) => (float)r.CellCount);
		}

		public static bool TryFindRandomReachableCellNear(IntVec3 root, float radius, TraverseParms traverseParms, Predicate<IntVec3> cellValidator, Predicate<Region> regionValidator, out IntVec3 result, int maxRegions = 999999)
		{
			Region region = root.GetRegion();
			if (region == null)
			{
				result = IntVec3.Invalid;
				return false;
			}
			CellFinder.workingRegions.Clear();
			float radSquared = radius * radius;
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, true) && (radius > 1000f || r.extentsClose.ClosestDistSquaredTo(root) <= radSquared) && (regionValidator == null || regionValidator(r)), delegate(Region r)
			{
				CellFinder.workingRegions.Add(r);
				return false;
			}, maxRegions);
			while (CellFinder.workingRegions.Count > 0)
			{
				Region region2 = CellFinder.workingRegions.RandomElementByWeight((Region r) => (float)r.CellCount);
				if (region2.TryFindRandomCellInRegion((IntVec3 c) => (c - root).LengthHorizontalSquared <= radSquared && cellValidator(c), out result))
				{
					return true;
				}
				CellFinder.workingRegions.Remove(region2);
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static IntVec3 RandomClosewalkCellNear(IntVec3 root, int radius)
		{
			IntVec3 result;
			if (!CellFinder.TryFindRandomReachableCellNear(root, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(), null, out result, 999999))
			{
				return root;
			}
			return result;
		}

		public static bool TryRandomClosewalkCellNear(IntVec3 root, int radius, out IntVec3 result)
		{
			return CellFinder.TryFindRandomReachableCellNear(root, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(), null, out result, 999999);
		}

		public static IntVec3 RandomClosewalkCellNear(IntVec3 root, int radius, Area area)
		{
			IntVec3 result;
			if (area == null || !CellFinder.TryFindRandomReachableCellNear(root, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => area[c] && c.Standable(), null, out result, 999999))
			{
				return CellFinder.RandomClosewalkCellNear(root, radius);
			}
			return result;
		}

		public static bool TryFindRandomCellInRegion(this Region reg, Predicate<IntVec3> validator, out IntVec3 result)
		{
			for (int i = 0; i < 10; i++)
			{
				result = reg.RandomCell;
				if (validator == null || validator(result))
				{
					return true;
				}
			}
			CellFinder.workingCells.Clear();
			CellFinder.workingCells.AddRange(reg.Cells);
			CellFinder.workingCells.Shuffle<IntVec3>();
			for (int j = 0; j < CellFinder.workingCells.Count; j++)
			{
				result = CellFinder.workingCells[j];
				if (validator == null || validator(result))
				{
					return true;
				}
			}
			result = reg.RandomCell;
			return false;
		}

		public static bool TryFindRandomCellNear(IntVec3 root, int squareRadius, Predicate<IntVec3> validator, out IntVec3 result)
		{
			int num = root.x - squareRadius;
			int num2 = root.x + squareRadius;
			int num3 = root.z - squareRadius;
			int num4 = root.z + squareRadius;
			if (num < 0)
			{
				num = 0;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num2 > Find.Map.Size.x)
			{
				num2 = Find.Map.Size.x;
			}
			if (num4 > Find.Map.Size.z)
			{
				num4 = Find.Map.Size.z;
			}
			int num5 = 0;
			IntVec3 intVec;
			while (true)
			{
				intVec = new IntVec3(Rand.RangeInclusive(num, num2), 0, Rand.RangeInclusive(num3, num4));
				if (validator == null || validator(intVec))
				{
					break;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					Find.DebugDrawer.FlashCell(intVec, 0f, "inv");
				}
				num5++;
				if (num5 > 20)
				{
					goto Block_8;
				}
			}
			if (DebugViewSettings.drawDestSearch)
			{
				Find.DebugDrawer.FlashCell(intVec, 0.5f, "found");
			}
			result = intVec;
			return true;
			Block_8:
			CellFinder.workingListX.Clear();
			CellFinder.workingListZ.Clear();
			for (int i = num; i <= num2; i++)
			{
				CellFinder.workingListX.Add(i);
			}
			for (int j = num3; j <= num4; j++)
			{
				CellFinder.workingListZ.Add(j);
			}
			CellFinder.workingListX.Shuffle<int>();
			CellFinder.workingListZ.Shuffle<int>();
			for (int k = 0; k < CellFinder.workingListX.Count; k++)
			{
				for (int l = 0; l < CellFinder.workingListZ.Count; l++)
				{
					intVec = new IntVec3(CellFinder.workingListX[k], 0, CellFinder.workingListZ[l]);
					if (validator(intVec))
					{
						if (DebugViewSettings.drawDestSearch)
						{
							Find.DebugDrawer.FlashCell(intVec, 0.6f, "found2");
						}
						result = intVec;
						return true;
					}
					if (DebugViewSettings.drawDestSearch)
					{
						Find.DebugDrawer.FlashCell(intVec, 0.25f, "inv2");
					}
				}
			}
			result = root;
			return false;
		}

		public static bool TryFindRandomPawnExitCell(Pawn searcher, out IntVec3 result)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !Find.RoofGrid.Roofed(c) && c.Walkable() && searcher.CanReach(c, PathEndMode.OnCell, Danger.Some, false, TraverseMode.ByPawn), out result);
		}

		public static bool TryFindRandomEdgeCellWith(Predicate<IntVec3> validator, out IntVec3 result)
		{
			for (int i = 0; i < 100; i++)
			{
				result = CellFinder.RandomEdgeCell();
				if (validator(result))
				{
					return true;
				}
			}
			if (CellFinder.mapEdgeCells == null || Find.Map.Size != CellFinder.mapEdgeCellsSize)
			{
				CellFinder.mapEdgeCellsSize = Find.Map.Size;
				CellFinder.mapEdgeCells = new List<IntVec3>();
				foreach (IntVec3 current in CellRect.WholeMap.EdgeCells)
				{
					CellFinder.mapEdgeCells.Add(current);
				}
			}
			CellFinder.mapEdgeCells.Shuffle<IntVec3>();
			for (int j = 0; j < CellFinder.mapEdgeCells.Count; j++)
			{
				try
				{
					if (validator(CellFinder.mapEdgeCells[j]))
					{
						result = CellFinder.mapEdgeCells[j];
						return true;
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"TryFindRandomEdgeCellWith exception validating ",
						CellFinder.mapEdgeCells[j],
						": ",
						ex.ToString()
					}));
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindBestPawnStandCell(Pawn forPawn, out IntVec3 cell)
		{
			cell = IntVec3.Invalid;
			int num = -1;
			float radius = 10f;
			while (true)
			{
				CellFinder.tmpDistances.Clear();
				Dijkstra<IntVec3>.Run(forPawn.Position, (IntVec3 x) => CellFinder.GetAdjacentCells(x, radius, forPawn), delegate(IntVec3 from, IntVec3 to)
				{
					float num3 = 1f;
					if (from.x != to.x && from.z != to.z)
					{
						num3 = 1.41421354f;
					}
					if (!to.Standable())
					{
						num3 += 3f;
					}
					if (PawnUtility.AnyPawnBlockingPathAt(to, forPawn, false, false))
					{
						bool flag = to.GetThingList().Find((Thing x) => x is Pawn && x.HostileTo(forPawn)) != null;
						if (flag)
						{
							num3 += 40f;
						}
						else
						{
							num3 += 15f;
						}
					}
					Building_Door building_Door2 = to.GetEdifice() as Building_Door;
					if (building_Door2 != null && !building_Door2.FreePassage)
					{
						num3 += 6f;
					}
					return num3;
				}, ref CellFinder.tmpDistances);
				if (CellFinder.tmpDistances.Count == num)
				{
					break;
				}
				float num2 = 0f;
				foreach (KeyValuePair<IntVec3, float> current in CellFinder.tmpDistances)
				{
					if (!cell.IsValid || current.Value < num2)
					{
						if (current.Key.Walkable())
						{
							if (!PawnUtility.AnyPawnBlockingPathAt(current.Key, forPawn, false, false))
							{
								Building_Door building_Door = current.Key.GetEdifice() as Building_Door;
								if (building_Door == null || building_Door.FreePassage)
								{
									cell = current.Key;
									num2 = current.Value;
								}
							}
						}
					}
				}
				if (cell.IsValid)
				{
					return true;
				}
				if (radius > (float)Find.Map.Size.x && radius > (float)Find.Map.Size.z)
				{
					return false;
				}
				radius *= 2f;
				num = CellFinder.tmpDistances.Count;
			}
			return false;
		}

		[DebuggerHidden]
		private static IEnumerable<IntVec3> GetAdjacentCells(IntVec3 x, float radius, Pawn pawn)
		{
			if ((float)(x - pawn.Position).LengthManhattan <= radius)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = x + GenAdj.AdjacentCells[i];
					if (c.InBounds() && c.Walkable())
					{
						Building_Door door = c.GetEdifice() as Building_Door;
						if (door == null || door.CanPhysicallyPass(pawn))
						{
							yield return c;
						}
					}
				}
			}
		}
	}
}
