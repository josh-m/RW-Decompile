using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class RCellFinder
	{
		public static IntVec3 BestOrderedGotoDestNear(IntVec3 root, Pawn searcher)
		{
			Map map = searcher.Map;
			Predicate<IntVec3> predicate = (IntVec3 c) => !map.pawnDestinationManager.DestinationIsReserved(c, searcher) && c.Standable(map) && searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
			if (predicate(root))
			{
				return root;
			}
			int num = 1;
			IntVec3 result = default(IntVec3);
			float num2 = -1000f;
			bool flag = false;
			while (true)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[num];
				if (predicate(intVec))
				{
					float num3 = CoverUtility.TotalSurroundingCoverScore(intVec, map);
					if (num3 > num2)
					{
						num2 = num3;
						result = intVec;
						flag = true;
					}
				}
				if (num >= 8 && flag)
				{
					break;
				}
				num++;
			}
			return result;
		}

		public static bool TryFindBestExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAnything && !pawn.Map.reachability.CanReachMapEdge(pawn.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, true)))
			{
				return RCellFinder.TryFindRandomPawnEntryCell(out spot, pawn.Map);
			}
			int num = 0;
			int num2 = 0;
			IntVec3 intVec2;
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					break;
				}
				IntVec3 intVec;
				bool flag = CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, num, null, out intVec);
				num += 4;
				if (flag)
				{
					int num3 = intVec.x;
					intVec2 = new IntVec3(0, 0, intVec.z);
					if (pawn.Map.Size.z - intVec.z < num3)
					{
						num3 = pawn.Map.Size.z - intVec.z;
						intVec2 = new IntVec3(intVec.x, 0, pawn.Map.Size.z - 1);
					}
					if (pawn.Map.Size.x - intVec.x < num3)
					{
						num3 = pawn.Map.Size.x - intVec.x;
						intVec2 = new IntVec3(pawn.Map.Size.x - 1, 0, intVec.z);
					}
					if (intVec.z < num3)
					{
						intVec2 = new IntVec3(intVec.x, 0, 0);
					}
					if (intVec2.Standable(pawn.Map) && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly, true, mode))
					{
						goto Block_9;
					}
				}
			}
			spot = pawn.Position;
			return false;
			Block_9:
			spot = intVec2;
			return true;
		}

		public static bool TryFindRandomExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			Danger maxDanger = Danger.Some;
			int num = 0;
			IntVec3 intVec;
			while (true)
			{
				num++;
				if (num > 40)
				{
					break;
				}
				if (num > 15)
				{
					maxDanger = Danger.Deadly;
				}
				intVec = CellFinder.RandomCell(pawn.Map);
				int num2 = Rand.RangeInclusive(0, 3);
				if (num2 == 0)
				{
					intVec.x = 0;
				}
				if (num2 == 1)
				{
					intVec.x = pawn.Map.Size.x - 1;
				}
				if (num2 == 2)
				{
					intVec.z = 0;
				}
				if (num2 == 3)
				{
					intVec.z = pawn.Map.Size.z - 1;
				}
				if (intVec.Standable(pawn.Map))
				{
					if (pawn.CanReach(intVec, PathEndMode.OnCell, maxDanger, false, mode))
					{
						goto IL_D5;
					}
				}
			}
			spot = pawn.Position;
			return false;
			IL_D5:
			spot = intVec;
			return true;
		}

		public static IntVec3 RandomWanderDestFor(Pawn pawn, IntVec3 root, float radius, Func<Pawn, IntVec3, bool> validator, Danger maxDanger)
		{
			if (radius > 12f)
			{
				Log.Warning(string.Concat(new object[]
				{
					"wanderRadius of ",
					radius,
					" is greater than Region.GridSize of ",
					12,
					" and will break."
				}));
			}
			bool drawDebug = UnityData.isDebugBuild && DebugViewSettings.drawDestSearch;
			if (drawDebug)
			{
				pawn.Map.debugDrawer.FlashCell(root, 0.6f, "root");
			}
			if (!root.Walkable(pawn.Map))
			{
				IntVec3 result;
				CellFinder.TryFindRandomCellNear(root, pawn.Map, 50, (IntVec3 c) => c.InBounds(pawn.Map), out result);
				return result;
			}
			Region region = root.GetRegion(pawn.Map);
			for (int j = Mathf.Max((int)radius / 3, 6); j >= 1; j--)
			{
				Region region2 = CellFinder.RandomRegionNear(region, j, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null);
				if (region2.extentsClose.ClosestDistSquaredTo(root) > radius * radius)
				{
					if (drawDebug)
					{
						pawn.Map.debugDrawer.FlashCell(region2.extentsClose.CenterCell, 0.36f, "region distance");
					}
				}
				else
				{
					int num = (j != 1) ? 1 : 3;
					int i;
					for (i = 0; i < num; i++)
					{
						IntVec3 intVec;
						if (region2.TryFindRandomCellInRegionUnforbidden(pawn, delegate(IntVec3 c)
						{
							if (!c.InHorDistOf(root, radius))
							{
								if (drawDebug)
								{
									pawn.Map.debugDrawer.FlashCell(c, 0.32f, "distance");
								}
								return false;
							}
							return RCellFinder.CanWanderToCell(c, pawn, root, validator, i >= 1, maxDanger);
						}, out intVec))
						{
							if (drawDebug)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.9f, "go!");
							}
							return intVec;
						}
					}
				}
			}
			return root;
		}

		private static bool CanWanderToCell(IntVec3 c, Pawn pawn, IntVec3 root, Func<Pawn, IntVec3, bool> validator, bool desperate, Danger maxDanger)
		{
			bool flag = UnityData.isDebugBuild && DebugViewSettings.drawDestSearch;
			if (!c.Walkable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0f, "walk");
				}
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "forbid");
				}
				return false;
			}
			if (!desperate && !c.Standable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "stand");
				}
				return false;
			}
			if (!pawn.CanReach(c, PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.6f, "reach");
				}
				return false;
			}
			if (RCellFinder.ContainsKnownTrap(c, pawn.Map, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.1f, "trap");
				}
				return false;
			}
			if (!desperate)
			{
				if (c.GetTerrain(pawn.Map).avoidWander)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.39f, "terr");
					}
					return false;
				}
				if (pawn.Map.pathGrid.PerceivedPathCostAt(c) > 20)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.4f, "pcost");
					}
					return false;
				}
			}
			Danger dangerFor = c.GetDangerFor(pawn);
			if (!desperate && dangerFor > Danger.None)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.4f, "danger");
				}
				return false;
			}
			if (desperate && dangerFor == Danger.Deadly)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.4f, "deadly");
				}
				return false;
			}
			if (pawn.Map.pawnDestinationManager.DestinationIsReserved(c, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.75f, "resvd");
				}
				return false;
			}
			if (validator != null && !validator(pawn, c))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.15f, "valid");
				}
				return false;
			}
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null && edifice.def.regionBarrier)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.32f, "barrier");
				}
				return false;
			}
			return true;
		}

		private static bool ContainsKnownTrap(IntVec3 c, Map map, Pawn pawn)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice != null)
			{
				Building_Trap building_Trap = edifice as Building_Trap;
				if (building_Trap != null && building_Trap.Armed && building_Trap.KnowsOfTrap(pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryFindGoodAdjacentSpotToTouch(Pawn toucher, Thing touchee, out IntVec3 result)
		{
			foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder(null))
			{
				if (current.Standable(toucher.Map) && !RCellFinder.ContainsKnownTrap(current, toucher.Map, toucher))
				{
					result = current;
					bool result2 = true;
					return result2;
				}
			}
			foreach (IntVec3 current2 in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder(null))
			{
				if (current2.Walkable(toucher.Map))
				{
					result = current2;
					bool result2 = true;
					return result2;
				}
			}
			result = touchee.Position;
			return false;
		}

		public static bool TryFindRandomPawnEntryCell(out IntVec3 result, Map map)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && !map.roofGrid.Roofed(c) && map.reachability.CanReachColony(c) && c.GetRoom(map).TouchesMapEdge, map, out result);
		}

		public static bool TryFindPrisonerReleaseCell(Pawn prisoner, Pawn warden, out IntVec3 result)
		{
			if (prisoner.Map != warden.Map)
			{
				result = IntVec3.Invalid;
				return false;
			}
			warden.Map.regionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
			Region validRegionAt = warden.Map.regionGrid.GetValidRegionAt(prisoner.Position);
			if (validRegionAt == null)
			{
				result = default(IntVec3);
				return false;
			}
			TraverseParms traverseParms = TraverseParms.For(warden, Danger.Deadly, TraverseMode.ByPawn, false);
			bool needMapEdge = prisoner.Faction != warden.Faction;
			IntVec3 foundResult = IntVec3.Invalid;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (needMapEdge)
				{
					if (!r.Room.TouchesMapEdge)
					{
						return false;
					}
				}
				else if (r.Room.isPrisonCell)
				{
					return false;
				}
				foundResult = r.RandomCell;
				return true;
			};
			RegionTraverser.BreadthFirstTraverse(validRegionAt, (Region from, Region r) => r.Allows(traverseParms, false), regionProcessor, 999);
			if (foundResult.IsValid)
			{
				result = foundResult;
				return true;
			}
			result = default(IntVec3);
			return false;
		}

		public static bool TryFindRandomCellToPlantInFromOffMap(ThingDef plantDef, Map map, out IntVec3 plantCell)
		{
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (c.Roofed(map))
				{
					return false;
				}
				if (!plantDef.CanEverPlantAt(c, map))
				{
					return false;
				}
				Room room = c.GetRoom(map);
				return room != null && room.TouchesMapEdge;
			};
			return CellFinder.TryFindRandomEdgeCellWith(validator, map, out plantCell);
		}

		public static IntVec3 RandomAnimalSpawnCell_MapGen(Map map)
		{
			int numStand = 0;
			int numRoom = 0;
			int numTouch = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					numStand++;
					return false;
				}
				if (c.GetTerrain(map).avoidWander)
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (room == null)
				{
					numRoom++;
					return false;
				}
				if (!room.TouchesMapEdge)
				{
					numTouch++;
					return false;
				}
				return true;
			};
			IntVec3 intVec;
			if (!CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out intVec))
			{
				Log.Message(string.Concat(new object[]
				{
					"RandomAnimalSpawnCell_MapGen failed: numStand=",
					numStand,
					", numRoom=",
					numRoom,
					", numTouch=",
					numTouch,
					". PlayerStartSpot=",
					MapGenerator.PlayerStartSpot,
					". Returning ",
					intVec
				}));
				Log.Message("RandomAnimalSpawnCell_MapGen failed");
			}
			return intVec;
		}

		public static bool TryFindSkygazeCell(IntVec3 root, Pawn searcher, out IntVec3 result)
		{
			Predicate<IntVec3> cellValidator = (IntVec3 c) => !c.Roofed(searcher.Map) && !c.GetTerrain(searcher.Map).avoidWander;
			IntVec3 unused;
			Predicate<Region> validator = (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(searcher) && r.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out unused);
			TraverseParms traverseParms = TraverseParms.For(searcher, Danger.Deadly, TraverseMode.ByPawn, false);
			Region root2;
			if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(searcher.Map), traverseParms, validator, 300, out root2))
			{
				result = root;
				return false;
			}
			Region reg = CellFinder.RandomRegionNear(root2, 14, traverseParms, validator, searcher);
			return reg.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out result);
		}

		public static bool TryFindTravelDestFrom(IntVec3 root, Map map, out IntVec3 travelDest)
		{
			travelDest = root;
			bool flag = false;
			Predicate<IntVec3> cellValidator = (IntVec3 c) => map.reachability.CanReach(root, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.None) && !map.roofGrid.Roofed(c);
			if (root.x == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == map.Size.x - 1 && cellValidator(c), map, out travelDest);
			}
			else if (root.x == map.Size.x - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == 0 && cellValidator(c), map, out travelDest);
			}
			else if (root.z == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == map.Size.z - 1 && cellValidator(c), map, out travelDest);
			}
			else if (root.z == map.Size.z - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == 0 && cellValidator(c), map, out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 10000f && cellValidator(c), map, out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 2500f && cellValidator(c), map, out travelDest);
			}
			return flag;
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 originCell, Map map, out IntVec3 result)
		{
			return RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, null, out result, null);
		}

		public static bool TryFindRandomSpotJustOutsideColony(Pawn searcher, out IntVec3 result)
		{
			return RCellFinder.TryFindRandomSpotJustOutsideColony(searcher.Position, searcher.Map, searcher, out result, null);
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 root, Map map, Pawn searcher, out IntVec3 result, Predicate<IntVec3> extraValidator = null)
		{
			bool desperate = false;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = c.GetRoom(map);
				return room.PsychologicallyOutdoors && room.TouchesMapEdge && room != null && room.CellCount >= 25 && (desperate || map.reachability.CanReachColony(c)) && (extraValidator == null || extraValidator(c));
			};
			for (int i = 0; i < 100; i++)
			{
				Building building = null;
				if (!(from b in map.listerBuildings.allBuildingsColonist
				where b.def.designationCategory != DesignationCategoryDefOf.Structure && b.def.building.ai_chillDestination
				select b).TryRandomElement(out building))
				{
					break;
				}
				int squareRadius = 10 + i / 5;
				desperate = (i > 60);
				if (CellFinder.TryFindRandomCellNear(building.Position, map, squareRadius, validator, out result))
				{
					return true;
				}
			}
			for (int j = 0; j < 50; j++)
			{
				Building building2 = null;
				if (!map.listerBuildings.allBuildingsColonist.TryRandomElement(out building2))
				{
					break;
				}
				desperate = (j > 20);
				if (CellFinder.TryFindRandomCellNear(building2.Position, map, 14, validator, out result))
				{
					return true;
				}
			}
			for (int k = 0; k < 100; k++)
			{
				Pawn pawn = null;
				if (!map.mapPawns.FreeColonistsAndPrisonersSpawned.TryRandomElement(out pawn))
				{
					break;
				}
				desperate = (k > 50);
				if (CellFinder.TryFindRandomCellNear(pawn.Position, map, 14, validator, out result))
				{
					return true;
				}
			}
			return CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out result);
		}

		public static bool TryFindRandomCellInRegionUnforbidden(this Region reg, Pawn pawn, Predicate<IntVec3> validator, out IntVec3 result)
		{
			if (reg == null)
			{
				throw new ArgumentNullException("reg");
			}
			if (reg.IsForbiddenEntirely(pawn))
			{
				result = IntVec3.Invalid;
				return false;
			}
			return reg.TryFindRandomCellInRegion((IntVec3 c) => !c.IsForbidden(pawn) && (validator == null || validator(c)), out result);
		}

		public static bool TryFindDirectFleeDestination(IntVec3 root, float dist, Pawn pawn, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				result = root + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * dist);
				if (result.Walkable(pawn.Map) && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, pawn.Map, true))
				{
					return true;
				}
			}
			Region region = pawn.GetRegion();
			for (int j = 0; j < 30; j++)
			{
				Region region2 = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null);
				IntVec3 randomCell = region2.RandomCell;
				if (randomCell.Walkable(pawn.Map) && (root - randomCell).LengthHorizontalSquared > dist * dist)
				{
					using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, randomCell, pawn, PathEndMode.OnCell))
					{
						if (PawnPathUtility.TryFindCellAtIndex(pawnPath, (int)dist + 3, out result))
						{
							return true;
						}
					}
				}
			}
			result = pawn.Position;
			return false;
		}

		public static bool TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(IntVec3 pos, Map map, float minDistToColony, out IntVec3 result)
		{
			int num = 30;
			CellRect cellRect = CellRect.CenteredOn(map.Center, num);
			cellRect.ClipInsideMap(map);
			List<IntVec3> list = new List<IntVec3>();
			if (minDistToColony > 0f)
			{
				foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
				{
					list.Add(current.Position);
				}
				foreach (Building current2 in map.listerBuildings.allBuildingsColonist)
				{
					list.Add(current2.Position);
				}
			}
			float num2 = minDistToColony * minDistToColony;
			int num3 = 0;
			IntVec3 randomCell;
			while (true)
			{
				num3++;
				if (num3 > 50)
				{
					if (num > map.Size.x)
					{
						break;
					}
					num = (int)((float)num * 1.5f);
					cellRect = CellRect.CenteredOn(map.Center, num);
					cellRect.ClipInsideMap(map);
					num3 = 0;
				}
				randomCell = cellRect.RandomCell;
				if (randomCell.Standable(map))
				{
					if (map.reachability.CanReach(randomCell, pos, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Deadly))
					{
						bool flag = false;
						for (int i = 0; i < list.Count; i++)
						{
							if ((list[i] - randomCell).LengthHorizontalSquared < num2)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							goto IL_19E;
						}
					}
				}
			}
			result = pos;
			return false;
			IL_19E:
			result = randomCell;
			return true;
		}

		public static bool TryFindRandomCellNearTheCenterOfTheMapWith(Predicate<IntVec3> validator, Map map, out IntVec3 result)
		{
			int startingSearchRadius = Mathf.Clamp(Mathf.Max(map.Size.x, map.Size.z) / 20, 3, 25);
			return RCellFinder.TryFindRandomCellNearWith(map.Center, validator, map, out result, startingSearchRadius);
		}

		public static bool TryFindRandomCellNearWith(IntVec3 near, Predicate<IntVec3> validator, Map map, out IntVec3 result, int startingSearchRadius = 5)
		{
			int num = startingSearchRadius;
			CellRect cellRect = CellRect.CenteredOn(near, num);
			cellRect.ClipInsideMap(map);
			int num2 = 0;
			IntVec3 randomCell;
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					if (num > map.Size.x * 2 && num > map.Size.z * 2)
					{
						break;
					}
					num = (int)((float)num * 1.5f);
					cellRect = CellRect.CenteredOn(near, num);
					cellRect.ClipInsideMap(map);
					num2 = 0;
				}
				randomCell = cellRect.RandomCell;
				if (validator(randomCell))
				{
					goto IL_8B;
				}
			}
			result = near;
			return false;
			IL_8B:
			result = randomCell;
			return true;
		}

		public static IntVec3 SpotToChewStandingNear(Pawn pawn, Thing ingestible)
		{
			IntVec3 root = pawn.Position;
			Room rootRoom = pawn.GetRoom();
			bool desperate = false;
			float maxDist = 4f;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if ((root - c).LengthHorizontalSquared > maxDist * maxDist)
				{
					return false;
				}
				if (pawn.HostFaction != null && c.GetRoom(pawn.Map) != rootRoom)
				{
					return false;
				}
				if (!desperate)
				{
					if (!c.Standable(pawn.Map))
					{
						return false;
					}
					if (GenPlace.HaulPlaceBlockerIn(null, c, pawn.Map, false) != null)
					{
						return false;
					}
					if (c.GetRegion(pawn.Map).portal != null)
					{
						return false;
					}
				}
				IntVec3 intVec2;
				return !c.ContainsStaticFire(pawn.Map) && !c.ContainsTrap(pawn.Map) && !pawn.Map.pawnDestinationManager.DestinationIsReserved(c, pawn) && Toils_Ingest.TryFindAdjacentIngestionPlaceSpot(c, ingestible.def, pawn, out intVec2);
			};
			int maxRegions = 1;
			Region region = pawn.GetRegion();
			for (int i = 0; i < 30; i++)
			{
				if (i == 1)
				{
					desperate = true;
				}
				else if (i == 2)
				{
					desperate = false;
					maxRegions = 4;
				}
				else if (i == 6)
				{
					desperate = true;
				}
				else if (i == 10)
				{
					desperate = false;
					maxDist = 8f;
					maxRegions = 12;
				}
				else if (i == 15)
				{
					desperate = true;
				}
				else if (i == 20)
				{
					maxDist = 15f;
					maxRegions = 16;
				}
				Region reg = CellFinder.RandomRegionNear(region, maxRegions, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null);
				IntVec3 intVec;
				if (reg.TryFindRandomCellInRegionUnforbidden(pawn, validator, out intVec))
				{
					if (DebugViewSettings.drawDestSearch)
					{
						pawn.Map.debugDrawer.FlashCell(intVec, 0.5f, "go!");
					}
					return intVec;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					pawn.Map.debugDrawer.FlashCell(intVec, 0f, i.ToString());
				}
			}
			return region.RandomCell;
		}

		public static bool TryFindMarriageSite(Pawn firstFiance, Pawn secondFiance, out IntVec3 result)
		{
			if (!firstFiance.CanReach(secondFiance, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				result = IntVec3.Invalid;
				return false;
			}
			Map map = firstFiance.Map;
			if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MarriageSpot)
			where MarriageSpotUtility.IsValidMarriageSpotFor(x.Position, firstFiance, secondFiance, null)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noMarriageSpotValidator = delegate(IntVec3 cell)
			{
				IntVec3 c = cell + LordToil_MarriageCeremony.OtherFianceNoMarriageSpotCellOffset;
				if (!c.InBounds(map))
				{
					return false;
				}
				if (c.IsForbidden(firstFiance) || c.IsForbidden(secondFiance))
				{
					return false;
				}
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = cell.GetRoom(map);
				return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
			};
			foreach (CompGatherSpot current in map.gatherSpotLister.activeSpots.InRandomOrder(null))
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, current.parent.Map, 4);
					if (MarriageSpotUtility.IsValidMarriageSpotFor(intVec, firstFiance, secondFiance, null) && noMarriageSpotValidator(intVec))
					{
						result = intVec;
						bool result2 = true;
						return result2;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(firstFiance.Position, firstFiance.Map, 25, (IntVec3 cell) => MarriageSpotUtility.IsValidMarriageSpotFor(cell, firstFiance, secondFiance, null) && noMarriageSpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindPartySpot(Pawn organizer, out IntVec3 result)
		{
			bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer, null);
			Map map = organizer.Map;
			Predicate<IntVec3> baseValidator = delegate(IntVec3 cell)
			{
				if (!cell.Standable(map))
				{
					return false;
				}
				if (cell.GetDangerFor(organizer) != Danger.None)
				{
					return false;
				}
				if (!enjoyableOutside && !cell.Roofed(map))
				{
					return false;
				}
				if (cell.IsForbidden(organizer))
				{
					return false;
				}
				if (!organizer.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None, 1))
				{
					return false;
				}
				Room room = cell.GetRoom(map);
				bool flag = room != null && room.isPrisonCell;
				return organizer.IsPrisoner == flag;
			};
			if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.PartySpot)
			where baseValidator(x.Position)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noPartySpotValidator = delegate(IntVec3 cell)
			{
				Room room = cell.GetRoom(map);
				return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
			};
			foreach (CompGatherSpot current in map.gatherSpotLister.activeSpots.InRandomOrder(null))
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, current.parent.Map, 4);
					if (baseValidator(intVec) && noPartySpotValidator(intVec))
					{
						result = intVec;
						bool result2 = true;
						return result2;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(organizer.Position, organizer.Map, 25, (IntVec3 cell) => baseValidator(cell) && noPartySpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		internal static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot, Map map)
		{
			for (int i = 70; i >= 20; i -= 10)
			{
				IntVec3 result;
				if (RCellFinder.TryFindSiegePosition(entrySpot, (float)i, map, out result))
				{
					return result;
				}
			}
			Log.Error(string.Concat(new object[]
			{
				"Could not find siege spot from ",
				entrySpot,
				", using ",
				entrySpot
			}));
			return entrySpot;
		}

		private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, out IntVec3 result)
		{
			CellRect cellRect = CellRect.CenteredOn(entrySpot, 60);
			cellRect.ClipInsideMap(map);
			cellRect = cellRect.ContractedBy(14);
			List<IntVec3> list = new List<IntVec3>();
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				list.Add(current.Position);
			}
			foreach (Building current2 in map.listerBuildings.allBuildingsColonistCombatTargets)
			{
				list.Add(current2.Position);
			}
			float num = minDistToColony * minDistToColony;
			int num2 = 0;
			IntVec3 randomCell;
			while (true)
			{
				num2++;
				if (num2 > 200)
				{
					break;
				}
				randomCell = cellRect.RandomCell;
				if (randomCell.Standable(map))
				{
					if (randomCell.SupportsStructureType(map, TerrainAffordance.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordance.Light))
					{
						if (map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some))
						{
							if (map.reachability.CanReachColony(randomCell))
							{
								bool flag = false;
								for (int i = 0; i < list.Count; i++)
								{
									if ((list[i] - randomCell).LengthHorizontalSquared < num)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									if (!randomCell.Roofed(map))
									{
										goto IL_1A6;
									}
								}
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
			IL_1A6:
			result = randomCell;
			return true;
		}
	}
}
