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
		public static bool TryFindBestExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAnything && !pawn.Position.CanReachMapEdge(TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, true)))
			{
				return RCellFinder.TryFindRandomPawnEntryCell(out spot);
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
				bool flag = CellFinder.TryFindRandomCellNear(pawn.Position, num, null, out intVec);
				num += 4;
				if (flag)
				{
					int num3 = intVec.x;
					intVec2 = new IntVec3(0, 0, intVec.z);
					if (Find.Map.Size.z - intVec.z < num3)
					{
						num3 = Find.Map.Size.z - intVec.z;
						intVec2 = new IntVec3(intVec.x, 0, Find.Map.Size.z - 1);
					}
					if (Find.Map.Size.x - intVec.x < num3)
					{
						num3 = Find.Map.Size.x - intVec.x;
						intVec2 = new IntVec3(Find.Map.Size.x - 1, 0, intVec.z);
					}
					if (intVec.z < num3)
					{
						intVec2 = new IntVec3(intVec.x, 0, 0);
					}
					if (intVec2.Standable() && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly, true, mode))
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
				intVec = CellFinder.RandomCell();
				int num2 = Rand.RangeInclusive(0, 3);
				if (num2 == 0)
				{
					intVec.x = 0;
				}
				if (num2 == 1)
				{
					intVec.x = Find.Map.Size.x - 1;
				}
				if (num2 == 2)
				{
					intVec.z = 0;
				}
				if (num2 == 3)
				{
					intVec.z = Find.Map.Size.z - 1;
				}
				if (intVec.Standable())
				{
					if (pawn.CanReach(intVec, PathEndMode.OnCell, maxDanger, false, mode))
					{
						goto IL_C7;
					}
				}
			}
			spot = pawn.Position;
			return false;
			IL_C7:
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
				Find.DebugDrawer.FlashCell(root, 0.6f, "root");
			}
			if (!root.Walkable())
			{
				IntVec3 result;
				CellFinder.TryFindRandomCellNear(root, 50, (IntVec3 c) => c.InBounds(), out result);
				return result;
			}
			Region region = root.GetRegion();
			for (int j = Mathf.Max((int)radius / 3, 6); j >= 1; j--)
			{
				Region region2 = CellFinder.RandomRegionNear(region, j, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null);
				if (region2.extentsClose.ClosestDistSquaredTo(root) > radius * radius)
				{
					if (drawDebug)
					{
						Find.DebugDrawer.FlashCell(region2.extentsClose.CenterCell, 0.36f, "region distance");
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
									Find.DebugDrawer.FlashCell(c, 0.32f, "distance");
								}
								return false;
							}
							return RCellFinder.CanWanderToCell(c, pawn, root, validator, i >= 1, maxDanger);
						}, out intVec))
						{
							if (drawDebug)
							{
								Find.DebugDrawer.FlashCell(intVec, 0.9f, "go!");
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
			if (!c.Walkable())
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0f, "walk");
				}
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.25f, "forbid");
				}
				return false;
			}
			if (!desperate && !c.Standable())
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.25f, "stand");
				}
				return false;
			}
			if (!pawn.CanReach(c, PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.6f, "reach");
				}
				return false;
			}
			if (RCellFinder.ContainsKnownTrap(c, pawn))
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.1f, "trap");
				}
				return false;
			}
			if (!desperate)
			{
				if (c.GetTerrain().avoidWander)
				{
					if (flag)
					{
						Find.DebugDrawer.FlashCell(c, 0.39f, "terr");
					}
					return false;
				}
				if (Find.PathGrid.PerceivedPathCostAt(c) > 20)
				{
					if (flag)
					{
						Find.DebugDrawer.FlashCell(c, 0.4f, "pcost");
					}
					return false;
				}
			}
			Danger dangerFor = c.GetDangerFor(pawn);
			if (!desperate && dangerFor > Danger.None)
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.4f, "danger");
				}
				return false;
			}
			if (desperate && dangerFor == Danger.Deadly)
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.4f, "deadly");
				}
				return false;
			}
			if (Find.PawnDestinationManager.DestinationIsReserved(c, pawn))
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.75f, "resvd");
				}
				return false;
			}
			if (validator != null && !validator(pawn, c))
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.15f, "valid");
				}
				return false;
			}
			Building edifice = c.GetEdifice();
			if (edifice != null && edifice.def.regionBarrier)
			{
				if (flag)
				{
					Find.DebugDrawer.FlashCell(c, 0.32f, "barrier");
				}
				return false;
			}
			return true;
		}

		private static bool ContainsKnownTrap(IntVec3 c, Pawn pawn)
		{
			Building edifice = c.GetEdifice();
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
				if (current.Standable() && !RCellFinder.ContainsKnownTrap(current, toucher))
				{
					result = current;
					bool result2 = true;
					return result2;
				}
			}
			foreach (IntVec3 current2 in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder(null))
			{
				if (current2.Walkable())
				{
					result = current2;
					bool result2 = true;
					return result2;
				}
			}
			result = touchee.Position;
			return false;
		}

		public static bool TryFindRandomPawnEntryCell(out IntVec3 result)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable() && !Find.RoofGrid.Roofed(c) && c.CanReachColony() && c.GetRoom().TouchesMapEdge, out result);
		}

		public static bool TryFindPrisonerReleaseCell(Pawn prisoner, Pawn warden, out IntVec3 result)
		{
			RegionAndRoomUpdater.RebuildDirtyRegionsAndRooms();
			Region validRegionAt = Find.RegionGrid.GetValidRegionAt(prisoner.Position);
			if (validRegionAt == null)
			{
				result = default(IntVec3);
				return false;
			}
			TraverseParms traverseParms = TraverseParms.For(warden, Danger.Deadly, TraverseMode.ByPawn, false);
			IntVec3 foundResult = IntVec3.Invalid;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (!r.Room.TouchesMapEdge)
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

		public static bool TryFindRandomCellToPlantInFromOffMap(ThingDef plantDef, out IntVec3 plantCell)
		{
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (c.Roofed())
				{
					return false;
				}
				if (!plantDef.CanEverPlantAt(c))
				{
					return false;
				}
				Room room = c.GetRoom();
				return room != null && room.TouchesMapEdge;
			};
			return CellFinder.TryFindRandomEdgeCellWith(validator, out plantCell);
		}

		public static IntVec3 RandomAnimalSpawnCell_MapGen()
		{
			int numStand = 0;
			int numRoom = 0;
			int numTouch = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable())
				{
					numStand++;
					return false;
				}
				if (c.GetTerrain().avoidWander)
				{
					return false;
				}
				Room room = c.GetRoom();
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
			if (!CellFinderLoose.TryGetRandomCellWith(validator, 1000, out intVec))
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
			Predicate<IntVec3> cellValidator = (IntVec3 c) => !c.Roofed() && !c.GetTerrain().avoidWander;
			IntVec3 unused;
			Predicate<Region> validator = (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(searcher) && r.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out unused);
			TraverseParms traverseParms = TraverseParms.For(searcher, Danger.Deadly, TraverseMode.ByPawn, false);
			Region root2;
			if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(), traverseParms, validator, 300, out root2))
			{
				result = root;
				return false;
			}
			Region reg = CellFinder.RandomRegionNear(root2, 14, traverseParms, validator, searcher);
			return reg.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out result);
		}

		public static bool TryFindTravelDestFrom(IntVec3 root, out IntVec3 travelDest)
		{
			travelDest = root;
			bool flag = false;
			Predicate<IntVec3> cellValidator = (IntVec3 c) => root.CanReach(c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.None) && !Find.RoofGrid.Roofed(c);
			if (root.x == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == Find.Map.Size.x - 1 && cellValidator(c), out travelDest);
			}
			else if (root.x == Find.Map.Size.x - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == 0 && cellValidator(c), out travelDest);
			}
			else if (root.z == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == Find.Map.Size.z - 1 && cellValidator(c), out travelDest);
			}
			else if (root.z == Find.Map.Size.z - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == 0 && cellValidator(c), out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 10000f && cellValidator(c), out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 2500f && cellValidator(c), out travelDest);
			}
			return flag;
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 originCell, out IntVec3 result)
		{
			return RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, null, out result);
		}

		public static bool TryFindRandomSpotJustOutsideColony(Pawn searcher, out IntVec3 result)
		{
			return RCellFinder.TryFindRandomSpotJustOutsideColony(searcher.Position, searcher, out result);
		}

		private static bool TryFindRandomSpotJustOutsideColony(IntVec3 root, Pawn searcher, out IntVec3 result)
		{
			TraverseParms traverseParms = (searcher == null) ? TraverseMode.PassDoors : TraverseParms.For(searcher, Danger.Deadly, TraverseMode.ByPawn, false);
			Region region;
			if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(), traverseParms, (Region r) => r.Room.PsychologicallyOutdoors, 300, out region))
			{
				result = root;
				return false;
			}
			IntVec3 closestOutdoorCell = region.RandomCell;
			bool desperate = false;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable())
				{
					return false;
				}
				if (Find.RoofGrid.Roofed(c))
				{
					return false;
				}
				if (!desperate && !c.CanReachColony())
				{
					return false;
				}
				if (!closestOutdoorCell.CanReach(c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.None))
				{
					return false;
				}
				Room room = c.GetRoom();
				return room != null && room.CellCount >= 25;
			};
			for (int i = 0; i < 400; i++)
			{
				Building building;
				Find.ListerBuildings.allBuildingsColonistCombatTargets.TryRandomElement(out building);
				Thing thing = building;
				if (thing == null && Find.ListerBuildings.allBuildingsColonist.Count > 0)
				{
					thing = Find.ListerBuildings.allBuildingsColonist.RandomElement<Building>();
				}
				if (thing == null && Find.MapPawns.FreeColonistsSpawnedCount > 0)
				{
					thing = Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
				}
				if (thing == null)
				{
					if (!Find.MapPawns.FreeColonistsAndPrisonersSpawned.Any<Pawn>())
					{
						break;
					}
					thing = Find.MapPawns.FreeColonistsAndPrisonersSpawned.RandomElement<Pawn>();
				}
				if (CellFinder.TryFindRandomCellNear(thing.Position, 12, validator, out result))
				{
					return true;
				}
			}
			if (CellFinderLoose.TryGetRandomCellWith(validator, 1000, out result))
			{
				return true;
			}
			desperate = true;
			return CellFinderLoose.TryGetRandomCellWith(validator, 1000, out result);
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
				if (result.Walkable() && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, true))
				{
					return true;
				}
			}
			Region region = pawn.Position.GetRegion();
			for (int j = 0; j < 30; j++)
			{
				Region region2 = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null);
				IntVec3 randomCell = region2.RandomCell;
				if (randomCell.Walkable() && (root - randomCell).LengthHorizontalSquared > dist * dist)
				{
					using (PawnPath pawnPath = PathFinder.FindPath(pawn.Position, randomCell, pawn, PathEndMode.OnCell))
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

		public static bool TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(IntVec3 pos, float minDistToColony, out IntVec3 result)
		{
			int num = 30;
			CellRect cellRect = CellRect.CenteredOn(Find.Map.Center, num);
			cellRect.ClipInsideMap();
			List<IntVec3> list = new List<IntVec3>();
			if (minDistToColony > 0f)
			{
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					list.Add(current.Position);
				}
				foreach (Building current2 in Find.ListerBuildings.allBuildingsColonist)
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
					if (num > Find.Map.Size.x)
					{
						break;
					}
					num = (int)((float)num * 1.5f);
					cellRect = CellRect.CenteredOn(Find.Map.Center, num);
					cellRect.ClipInsideMap();
					num3 = 0;
				}
				randomCell = cellRect.RandomCell;
				if (randomCell.Standable())
				{
					if (randomCell.CanReach(pos, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Deadly))
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
							goto IL_19F;
						}
					}
				}
			}
			result = pos;
			return false;
			IL_19F:
			result = randomCell;
			return true;
		}

		public static IntVec3 SpotToChewStandingNear(Pawn pawn, Thing ingestible)
		{
			IntVec3 root = pawn.Position;
			Room rootRoom = root.GetRoom();
			bool desperate = false;
			float maxDist = 4f;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if ((root - c).LengthHorizontalSquared > maxDist * maxDist)
				{
					return false;
				}
				if (pawn.HostFaction != null && c.GetRoom() != rootRoom)
				{
					return false;
				}
				if (!desperate)
				{
					if (!c.Standable())
					{
						return false;
					}
					if (GenPlace.HaulPlaceBlockerIn(null, c, false) != null)
					{
						return false;
					}
					if (c.GetRegion().portal != null)
					{
						return false;
					}
				}
				IntVec3 intVec2;
				return !c.ContainsStaticFire() && !c.ContainsTrap() && !Find.PawnDestinationManager.DestinationIsReserved(c, pawn) && Toils_Ingest.TryFindAdjacentIngestionPlaceSpot(c, ingestible.def, pawn, out intVec2);
			};
			int maxRegions = 1;
			Region region = pawn.Position.GetRegion();
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
						Find.DebugDrawer.FlashCell(intVec, 0.5f, "go!");
					}
					return intVec;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					Find.DebugDrawer.FlashCell(intVec, 0f, i.ToString());
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
			if ((from x in Find.ListerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MarriageSpot)
			where MarriageSpotUtility.IsValidMarriageSpotFor(x.Position, firstFiance, secondFiance, null)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noMarriageSpotValidator = delegate(IntVec3 cell)
			{
				IntVec3 c = cell + LordToil_MarriageCeremony.OtherFianceNoMarriageSpotCellOffset;
				if (!c.InBounds())
				{
					return false;
				}
				if (c.IsForbidden(firstFiance) || c.IsForbidden(secondFiance))
				{
					return false;
				}
				if (!c.Standable())
				{
					return false;
				}
				Room room = cell.GetRoom();
				return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
			};
			foreach (CompGatherSpot current in GatherSpotLister.activeSpots.InRandomOrder(null))
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, 4);
					if (MarriageSpotUtility.IsValidMarriageSpotFor(intVec, firstFiance, secondFiance, null) && noMarriageSpotValidator(intVec))
					{
						result = intVec;
						bool result2 = true;
						return result2;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(firstFiance.Position, 25, (IntVec3 cell) => MarriageSpotUtility.IsValidMarriageSpotFor(cell, firstFiance, secondFiance, null) && noMarriageSpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindPartySpot(Pawn organizer, out IntVec3 result)
		{
			bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer, null);
			Predicate<IntVec3> baseValidator = delegate(IntVec3 cell)
			{
				if (!cell.Standable())
				{
					return false;
				}
				if (cell.GetDangerFor(organizer) != Danger.None)
				{
					return false;
				}
				if (!enjoyableOutside && !cell.Roofed())
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
				Room room = cell.GetRoom();
				bool flag = room != null && room.isPrisonCell;
				return organizer.IsPrisoner == flag;
			};
			if ((from x in Find.ListerBuildings.AllBuildingsColonistOfDef(ThingDefOf.PartySpot)
			where baseValidator(x.Position)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noPartySpotValidator = delegate(IntVec3 cell)
			{
				Room room = cell.GetRoom();
				return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
			};
			foreach (CompGatherSpot current in GatherSpotLister.activeSpots.InRandomOrder(null))
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, 4);
					if (baseValidator(intVec) && noPartySpotValidator(intVec))
					{
						result = intVec;
						bool result2 = true;
						return result2;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(organizer.Position, 25, (IntVec3 cell) => baseValidator(cell) && noPartySpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		internal static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot)
		{
			for (int i = 70; i >= 20; i -= 10)
			{
				IntVec3 result;
				if (RCellFinder.TryFindSiegePosition(entrySpot, (float)i, out result))
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

		private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, out IntVec3 result)
		{
			CellRect cellRect = CellRect.CenteredOn(entrySpot, 60);
			cellRect.ClipInsideMap();
			cellRect = cellRect.ContractedBy(14);
			List<IntVec3> list = new List<IntVec3>();
			foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
			{
				list.Add(current.Position);
			}
			foreach (Building current2 in Find.ListerBuildings.allBuildingsColonistCombatTargets)
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
				if (randomCell.Standable())
				{
					if (randomCell.SupportsStructureType(TerrainAffordance.Heavy) && randomCell.SupportsStructureType(TerrainAffordance.Light))
					{
						if (randomCell.CanReach(entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some))
						{
							if (randomCell.CanReachColony())
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
									if (!randomCell.Roofed())
									{
										goto IL_193;
									}
								}
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
			IL_193:
			result = randomCell;
			return true;
		}
	}
}
