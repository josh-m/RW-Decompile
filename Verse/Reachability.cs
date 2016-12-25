using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class Reachability
	{
		private static Queue<Region> openQueue = new Queue<Region>();

		private static List<Region> startingRegions = new List<Region>();

		private static List<Region> destRegions = new List<Region>();

		private static uint reachedIndex = 1u;

		private static int numRegionsOpened;

		private static bool working = false;

		private static List<IntVec3> neighList = new List<IntVec3>();

		private static ReachabilityCache cache = new ReachabilityCache();

		private static PathGrid pathGrid;

		private static RegionGrid regionGrid;

		public static void ClearCache()
		{
			if (Reachability.cache.Count > 0)
			{
				Reachability.cache.Clear();
			}
		}

		private static void QueueNewOpenRegion(Region region)
		{
			if (region == null)
			{
				Log.ErrorOnce("Tried to queue null region.", 881121);
				return;
			}
			if (region.reachedIndex == Reachability.reachedIndex)
			{
				Log.ErrorOnce("Region is already reached; you can't open it. Region: " + region.ToString(), 719991);
				return;
			}
			Reachability.openQueue.Enqueue(region);
			region.reachedIndex = Reachability.reachedIndex;
			Reachability.numRegionsOpened++;
		}

		private static uint NewReachedIndex()
		{
			return Reachability.reachedIndex++;
		}

		private static void FinalizeCheck()
		{
			Reachability.working = false;
		}

		public static bool CanReach(this Pawn pawn, TargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBash = false, TraverseMode mode = TraverseMode.ByPawn)
		{
			return pawn.Position.CanReach(dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBash));
		}

		public static bool CanReach(this IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			return start.CanReach(dest, peMode, TraverseParms.For(traverseMode, maxDanger, false));
		}

		public static bool CanReach(this IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			if (!start.InBounds() || !dest.Cell.InBounds())
			{
				return false;
			}
			if ((peMode == PathEndMode.OnCell || peMode == PathEndMode.Touch || peMode == PathEndMode.ClosestTouch) && RoomQuery.RoomAtFast(start) == RoomQuery.RoomAtFast(dest.Cell))
			{
				return true;
			}
			if (traverseParams.mode == TraverseMode.PassAnything)
			{
				return true;
			}
			if (traverseParams.pawn != null && traverseParams.pawn.carrier != null && dest.HasThing && traverseParams.pawn.carrier.CarriedThing == dest.Thing)
			{
				return true;
			}
			if (Reachability.working)
			{
				Log.ErrorOnce("Called ReachableBetween while working.", 7452341);
				return true;
			}
			GenPath.ResolvePathMode(ref dest, ref peMode);
			Reachability.working = true;
			Reachability.pathGrid = Find.PathGrid;
			Reachability.regionGrid = Find.RegionGrid;
			Reachability.reachedIndex += 1u;
			Reachability.destRegions.Clear();
			if (peMode == PathEndMode.OnCell)
			{
				Region validRegionAt = Reachability.regionGrid.GetValidRegionAt(dest.Cell);
				if (validRegionAt != null && validRegionAt.Allows(traverseParams, true))
				{
					Reachability.destRegions.Add(validRegionAt);
				}
			}
			else if (peMode == PathEndMode.Touch)
			{
				if (!dest.HasThing || (dest.Thing.def.size.x == 1 && dest.Thing.def.size.z == 1))
				{
					IntVec3 cell = dest.Cell;
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c = GenAdj.AdjacentCells[i] + cell;
						if (c.InBounds())
						{
							Region validRegionAt2 = Reachability.regionGrid.GetValidRegionAt(c);
							if (validRegionAt2 != null && validRegionAt2.Allows(traverseParams, true))
							{
								Reachability.destRegions.Add(validRegionAt2);
							}
						}
					}
				}
				else
				{
					Reachability.neighList = GenAdjFast.AdjacentCells8Way(dest);
					for (int j = 0; j < Reachability.neighList.Count; j++)
					{
						if (Reachability.neighList[j].InBounds())
						{
							Region validRegionAt3 = Reachability.regionGrid.GetValidRegionAt(Reachability.neighList[j]);
							if (validRegionAt3 != null && validRegionAt3.Allows(traverseParams, true))
							{
								Reachability.destRegions.Add(validRegionAt3);
							}
						}
					}
				}
			}
			if (Reachability.destRegions.Count == 0)
			{
				Reachability.FinalizeCheck();
				return false;
			}
			Reachability.destRegions.RemoveDuplicates<Region>();
			Reachability.openQueue.Clear();
			Reachability.numRegionsOpened = 0;
			Reachability.startingRegions.Clear();
			if (Reachability.pathGrid.WalkableFast(start))
			{
				Region validRegionAt4 = Reachability.regionGrid.GetValidRegionAt(start);
				Reachability.QueueNewOpenRegion(validRegionAt4);
				Reachability.startingRegions.Add(validRegionAt4);
			}
			else
			{
				for (int k = 0; k < 8; k++)
				{
					IntVec3 intVec = start + GenAdj.AdjacentCells[k];
					if (intVec.InBounds())
					{
						if (Reachability.pathGrid.WalkableFast(intVec))
						{
							Region validRegionAt5 = Reachability.regionGrid.GetValidRegionAt(intVec);
							if (validRegionAt5 != null && validRegionAt5.reachedIndex != Reachability.reachedIndex)
							{
								Reachability.QueueNewOpenRegion(validRegionAt5);
								Reachability.startingRegions.Add(validRegionAt5);
							}
						}
					}
				}
				if (Reachability.openQueue.Count == 0)
				{
					Reachability.FinalizeCheck();
					return false;
				}
			}
			bool flag = false;
			for (int l = 0; l < Reachability.startingRegions.Count; l++)
			{
				for (int m = 0; m < Reachability.destRegions.Count; m++)
				{
					if (Reachability.destRegions[m] == Reachability.startingRegions[l])
					{
						Reachability.FinalizeCheck();
						return true;
					}
					BoolUnknown boolUnknown = Reachability.cache.CachedResultFor(Reachability.startingRegions[l].Room, Reachability.destRegions[m].Room, traverseParams);
					if (boolUnknown == BoolUnknown.True)
					{
						Reachability.FinalizeCheck();
						return true;
					}
					if (boolUnknown == BoolUnknown.Unknown)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Reachability.FinalizeCheck();
				return false;
			}
			while (Reachability.openQueue.Count > 0)
			{
				Region region = Reachability.openQueue.Dequeue();
				for (int n = 0; n < region.links.Count; n++)
				{
					RegionLink regionLink = region.links[n];
					for (int num = 0; num < 2; num++)
					{
						Region region2 = regionLink.regions[num];
						if (region2 != null && region2.reachedIndex != Reachability.reachedIndex)
						{
							if (region2.Allows(traverseParams, false))
							{
								if (Reachability.destRegions.Contains(region2))
								{
									for (int num2 = 0; num2 < Reachability.startingRegions.Count; num2++)
									{
										Reachability.cache.AddCachedResult(Reachability.startingRegions[num2].Room, region2.Room, traverseParams, true);
									}
									Reachability.FinalizeCheck();
									return true;
								}
								Reachability.QueueNewOpenRegion(region2);
							}
						}
					}
				}
			}
			for (int num3 = 0; num3 < Reachability.startingRegions.Count; num3++)
			{
				for (int num4 = 0; num4 < Reachability.destRegions.Count; num4++)
				{
					Reachability.cache.AddCachedResult(Reachability.startingRegions[num3].Room, Reachability.destRegions[num4].Room, traverseParams, false);
				}
			}
			Reachability.FinalizeCheck();
			return false;
		}

		public static bool CanReachColony(this IntVec3 c)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return c.CanReach(MapGenerator.PlayerStartSpot, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false));
			}
			if (!c.Walkable())
			{
				return false;
			}
			List<Pawn> list = Find.MapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return true;
				}
			}
			List<Building> allBuildingsColonist = Find.ListerBuildings.allBuildingsColonist;
			for (int j = 0; j < allBuildingsColonist.Count; j++)
			{
				if (c.CanReach(allBuildingsColonist[j], PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
				{
					return true;
				}
			}
			return false;
		}

		public static bool CanReachMapEdge(Pawn p)
		{
			if (!p.Spawned)
			{
				Log.Error("Cannot do CanReachMapEdge on unspawned pawn " + p);
				return false;
			}
			return p.Position.CanReachMapEdge(TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false));
		}

		public static bool CanReachMapEdge(this IntVec3 c, TraverseParms traverseParms)
		{
			Region region = c.GetRegion();
			if (region == null)
			{
				return false;
			}
			if (region.Room.TouchesMapEdge)
			{
				return true;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
			bool foundReg = false;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.Room.TouchesMapEdge)
				{
					foundReg = true;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999);
			return foundReg;
		}
	}
}
