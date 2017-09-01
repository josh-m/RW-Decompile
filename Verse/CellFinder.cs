using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class CellFinder
	{
		public static float EdgeRoadChance_Ignore = 0f;

		public static float EdgeRoadChance_Animal = 0f;

		public static float EdgeRoadChance_Hostile = 0.2f;

		public static float EdgeRoadChance_Neutral = 0.75f;

		public static float EdgeRoadChance_Friendly = 0.75f;

		public static float EdgeRoadChance_Always = 1f;

		private static List<IntVec3> workingCells = new List<IntVec3>();

		private static List<Region> workingRegions = new List<Region>();

		private static List<int> workingListX = new List<int>();

		private static List<int> workingListZ = new List<int>();

		private static List<IntVec3> mapEdgeCells;

		private static IntVec3 mapEdgeCellsSize;

		private static List<IntVec3>[] mapSingleEdgeCells = new List<IntVec3>[4];

		private static IntVec3 mapSingleEdgeCellsSize;

		private static List<KeyValuePair<IntVec3, float>> tmpDistances = new List<KeyValuePair<IntVec3, float>>();

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		public static IntVec3 RandomCell(Map map)
		{
			return new IntVec3(Rand.Range(0, map.Size.x), 0, Rand.Range(0, map.Size.z));
		}

		public static IntVec3 RandomEdgeCell(Map map)
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
					result.x = map.Size.x - 1;
				}
				result.z = Rand.Range(0, map.Size.z);
			}
			else
			{
				if (Rand.Value < 0.5f)
				{
					result.z = 0;
				}
				else
				{
					result.z = map.Size.z - 1;
				}
				result.x = Rand.Range(0, map.Size.x);
			}
			return result;
		}

		public static IntVec3 RandomEdgeCell(Rot4 dir, Map map)
		{
			if (dir == Rot4.North)
			{
				return new IntVec3(Rand.Range(0, map.Size.x), 0, map.Size.z - 1);
			}
			if (dir == Rot4.South)
			{
				return new IntVec3(Rand.Range(0, map.Size.x), 0, 0);
			}
			if (dir == Rot4.West)
			{
				return new IntVec3(0, 0, Rand.Range(0, map.Size.z));
			}
			if (dir == Rot4.East)
			{
				return new IntVec3(map.Size.x - 1, 0, Rand.Range(0, map.Size.z));
			}
			return IntVec3.Invalid;
		}

		public static IntVec3 RandomNotEdgeCell(int minEdgeDistance, Map map)
		{
			if (minEdgeDistance > map.Size.x / 2 || minEdgeDistance > map.Size.z / 2)
			{
				return IntVec3.Invalid;
			}
			int newX = Rand.Range(minEdgeDistance, map.Size.x - minEdgeDistance);
			int newZ = Rand.Range(minEdgeDistance, map.Size.z - minEdgeDistance);
			return new IntVec3(newX, 0, newZ);
		}

		public static bool TryFindClosestRegionWith(Region rootReg, TraverseParms traverseParms, Predicate<Region> validator, int maxRegions, out Region result, RegionType traversableRegionTypes = RegionType.Set_Passable)
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
			}, maxRegions, traversableRegionTypes);
			result = localResult;
			return result != null;
		}

		public static Region RandomRegionNear(Region root, int maxRegions, TraverseParms traverseParms, Predicate<Region> validator = null, Pawn pawnToAllow = null, RegionType traversableRegionTypes = RegionType.Set_Passable)
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
			}, maxRegions, traversableRegionTypes);
			Region result = CellFinder.workingRegions.RandomElementByWeight((Region r) => (float)r.CellCount);
			CellFinder.workingRegions.Clear();
			return result;
		}

		public static void AllRegionsNear(List<Region> results, Region root, int maxRegions, TraverseParms traverseParms, Predicate<Region> validator = null, Pawn pawnToAllow = null, RegionType traversableRegionTypes = RegionType.Set_Passable)
		{
			if (results == null)
			{
				Log.ErrorOnce("Attempted to call AllRegionsNear with an invalid results list", 60733193);
				return;
			}
			results.Clear();
			if (root == null)
			{
				Log.ErrorOnce("Attempted to call AllRegionsNear with an invalid root", 9107839);
				return;
			}
			RegionTraverser.BreadthFirstTraverse(root, (Region from, Region r) => (validator == null || validator(r)) && r.Allows(traverseParms, true) && (pawnToAllow == null || !r.IsForbiddenEntirely(pawnToAllow)), delegate(Region r)
			{
				results.Add(r);
				return false;
			}, maxRegions, traversableRegionTypes);
		}

		public static bool TryFindRandomReachableCellNear(IntVec3 root, Map map, float radius, TraverseParms traverseParms, Predicate<IntVec3> cellValidator, Predicate<Region> regionValidator, out IntVec3 result, int maxRegions = 999999)
		{
			if (map == null)
			{
				Log.ErrorOnce("Tried to find reachable cell in a null map", 61037855);
				result = IntVec3.Invalid;
				return false;
			}
			Region region = root.GetRegion(map, RegionType.Set_Passable);
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
			}, maxRegions, RegionType.Set_Passable);
			while (CellFinder.workingRegions.Count > 0)
			{
				Region region2 = CellFinder.workingRegions.RandomElementByWeight((Region r) => (float)r.CellCount);
				if (region2.TryFindRandomCellInRegion((IntVec3 c) => (float)(c - root).LengthHorizontalSquared <= radSquared && cellValidator(c), out result))
				{
					CellFinder.workingRegions.Clear();
					return true;
				}
				CellFinder.workingRegions.Remove(region2);
			}
			result = IntVec3.Invalid;
			CellFinder.workingRegions.Clear();
			return false;
		}

		public static IntVec3 RandomClosewalkCellNear(IntVec3 root, Map map, int radius, Predicate<IntVec3> extraValidator = null)
		{
			IntVec3 result;
			if (CellFinder.TryRandomClosewalkCellNear(root, map, radius, out result, extraValidator))
			{
				return result;
			}
			return root;
		}

		public static bool TryRandomClosewalkCellNear(IntVec3 root, Map map, int radius, out IntVec3 result, Predicate<IntVec3> extraValidator = null)
		{
			return CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(map) && (extraValidator == null || extraValidator(c)), null, out result, 999999);
		}

		public static IntVec3 RandomClosewalkCellNearNotForbidden(IntVec3 root, Map map, int radius, Pawn pawn)
		{
			IntVec3 result;
			if (!CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => !c.IsForbidden(pawn) && c.Standable(map), null, out result, 999999))
			{
				return CellFinder.RandomClosewalkCellNear(root, map, radius, null);
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

		public static bool TryFindRandomCellNear(IntVec3 root, Map map, int squareRadius, Predicate<IntVec3> validator, out IntVec3 result)
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
			if (num2 > map.Size.x)
			{
				num2 = map.Size.x;
			}
			if (num4 > map.Size.z)
			{
				num4 = map.Size.z;
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
					map.debugDrawer.FlashCell(intVec, 0f, "inv");
				}
				num5++;
				if (num5 > 20)
				{
					goto Block_8;
				}
			}
			if (DebugViewSettings.drawDestSearch)
			{
				map.debugDrawer.FlashCell(intVec, 0.5f, "found");
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
							map.debugDrawer.FlashCell(intVec, 0.6f, "found2");
						}
						result = intVec;
						return true;
					}
					if (DebugViewSettings.drawDestSearch)
					{
						map.debugDrawer.FlashCell(intVec, 0.25f, "inv2");
					}
				}
			}
			result = root;
			return false;
		}

		public static bool TryFindRandomPawnExitCell(Pawn searcher, out IntVec3 result)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !searcher.Map.roofGrid.Roofed(c) && c.Walkable(searcher.Map) && searcher.CanReach(c, PathEndMode.OnCell, Danger.Some, false, TraverseMode.ByPawn), searcher.Map, 0f, out result);
		}

		public static bool TryFindRandomEdgeCellWith(Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result)
		{
			if (Rand.Value < roadChance)
			{
				bool flag = (from c in map.roadInfo.roadEdgeTiles
				where validator(c)
				select c).TryRandomElement(out result);
				if (flag)
				{
					return flag;
				}
			}
			for (int i = 0; i < 100; i++)
			{
				result = CellFinder.RandomEdgeCell(map);
				if (validator(result))
				{
					return true;
				}
			}
			if (CellFinder.mapEdgeCells == null || map.Size != CellFinder.mapEdgeCellsSize)
			{
				CellFinder.mapEdgeCellsSize = map.Size;
				CellFinder.mapEdgeCells = new List<IntVec3>();
				foreach (IntVec3 current in CellRect.WholeMap(map).EdgeCells)
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

		public static bool TryFindRandomEdgeCellWith(Predicate<IntVec3> validator, Map map, Rot4 dir, float roadChance, out IntVec3 result)
		{
			if (Rand.Value < roadChance)
			{
				bool flag = (from c in map.roadInfo.roadEdgeTiles
				where validator(c) && c.OnEdge(map, dir)
				select c).TryRandomElement(out result);
				if (flag)
				{
					return flag;
				}
			}
			for (int i = 0; i < 100; i++)
			{
				result = CellFinder.RandomEdgeCell(dir, map);
				if (validator(result))
				{
					return true;
				}
			}
			int asInt = dir.AsInt;
			if (CellFinder.mapSingleEdgeCells[asInt] == null || map.Size != CellFinder.mapSingleEdgeCellsSize)
			{
				CellFinder.mapSingleEdgeCellsSize = map.Size;
				CellFinder.mapSingleEdgeCells[asInt] = new List<IntVec3>();
				foreach (IntVec3 current in CellRect.WholeMap(map).GetEdgeCells(dir))
				{
					CellFinder.mapSingleEdgeCells[asInt].Add(current);
				}
			}
			List<IntVec3> list = CellFinder.mapSingleEdgeCells[asInt];
			list.Shuffle<IntVec3>();
			int j = 0;
			int count = list.Count;
			while (j < count)
			{
				try
				{
					if (validator(list[j]))
					{
						result = list[j];
						return true;
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"TryFindRandomEdgeCellWith exception validating ",
						list[j],
						": ",
						ex.ToString()
					}));
				}
				j++;
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
					if (!to.Standable(forPawn.Map))
					{
						num3 += 3f;
					}
					if (PawnUtility.AnyPawnBlockingPathAt(to, forPawn, false, false))
					{
						bool flag = to.GetThingList(forPawn.Map).Find((Thing x) => x is Pawn && x.HostileTo(forPawn)) != null;
						if (flag)
						{
							num3 += 40f;
						}
						else
						{
							num3 += 15f;
						}
					}
					Building_Door building_Door2 = to.GetEdifice(forPawn.Map) as Building_Door;
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
						if (current.Key.Walkable(forPawn.Map))
						{
							if (!PawnUtility.AnyPawnBlockingPathAt(current.Key, forPawn, false, false))
							{
								Building_Door building_Door = current.Key.GetEdifice(forPawn.Map) as Building_Door;
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
				if (radius > (float)forPawn.Map.Size.x && radius > (float)forPawn.Map.Size.z)
				{
					return false;
				}
				radius *= 2f;
				num = CellFinder.tmpDistances.Count;
			}
			return false;
		}

		public static bool TryFindRandomCellInsideWith(CellRect cellRect, Predicate<IntVec3> predicate, out IntVec3 result)
		{
			int area = cellRect.Area;
			int num = Mathf.Max(Mathf.RoundToInt(Mathf.Sqrt((float)area)), 5);
			for (int i = 0; i < num; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				if (predicate(randomCell))
				{
					result = randomCell;
					return true;
				}
			}
			CellFinder.tmpCells.Clear();
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				CellFinder.tmpCells.Add(iterator.Current);
				iterator.MoveNext();
			}
			CellFinder.tmpCells.Shuffle<IntVec3>();
			int j = 0;
			int count = CellFinder.tmpCells.Count;
			while (j < count)
			{
				if (predicate(CellFinder.tmpCells[j]))
				{
					result = CellFinder.tmpCells[j];
					return true;
				}
				j++;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static IntVec3 RandomSpawnCellForPawnNear(IntVec3 root, Map map, int firstTryWithRadius = 4)
		{
			IntVec3 result;
			if (CellFinder.TryFindRandomSpawnCellForPawnNear(root, map, out result, firstTryWithRadius))
			{
				return result;
			}
			return root;
		}

		public static bool TryFindRandomSpawnCellForPawnNear(IntVec3 root, Map map, out IntVec3 result, int firstTryWithRadius = 4)
		{
			if (root.Standable(map) && root.GetFirstPawn(map) == null)
			{
				result = root;
				return true;
			}
			bool rootFogged = root.Fogged(map);
			int num = firstTryWithRadius;
			for (int i = 0; i < 3; i++)
			{
				if (CellFinder.TryFindRandomReachableCellNear(root, map, (float)num, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(map) && (rootFogged || !c.Fogged(map)) && c.GetFirstPawn(map) == null, null, out result, 999999))
				{
					return true;
				}
				num *= 2;
			}
			num = firstTryWithRadius + 1;
			while (!CellFinder.TryRandomClosewalkCellNear(root, map, num, out result, null))
			{
				if (num > map.Size.x / 2 && num > map.Size.z / 2)
				{
					result = root;
					return false;
				}
				num *= 2;
			}
			return true;
		}

		[DebuggerHidden]
		private static IEnumerable<IntVec3> GetAdjacentCells(IntVec3 x, float radius, Pawn pawn)
		{
			if ((float)(x - pawn.Position).LengthManhattan <= radius)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = x + GenAdj.AdjacentCells[i];
					if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
					{
						Building_Door door = c.GetEdifice(pawn.Map) as Building_Door;
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
