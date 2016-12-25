using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public class Reachability
	{
		private Map map;

		private Queue<Region> openQueue = new Queue<Region>();

		private List<Region> startingRegions = new List<Region>();

		private List<Region> destRegions = new List<Region>();

		private uint reachedIndex = 1u;

		private int numRegionsOpened;

		private bool working;

		private List<IntVec3> neighList = new List<IntVec3>();

		private ReachabilityCache cache = new ReachabilityCache();

		private PathGrid pathGrid;

		private RegionGrid regionGrid;

		public Reachability(Map map)
		{
			this.map = map;
		}

		public void ClearCache()
		{
			if (this.cache.Count > 0)
			{
				this.cache.Clear();
			}
		}

		private void QueueNewOpenRegion(Region region)
		{
			if (region == null)
			{
				Log.ErrorOnce("Tried to queue null region.", 881121);
				return;
			}
			if (region.reachedIndex == this.reachedIndex)
			{
				Log.ErrorOnce("Region is already reached; you can't open it. Region: " + region.ToString(), 719991);
				return;
			}
			this.openQueue.Enqueue(region);
			region.reachedIndex = this.reachedIndex;
			this.numRegionsOpened++;
		}

		private uint NewReachedIndex()
		{
			return this.reachedIndex++;
		}

		private void FinalizeCheck()
		{
			this.working = false;
		}

		public bool CanReachNonLocal(IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			return dest.Map == this.map && this.CanReach(start, (LocalTargetInfo)dest, peMode, traverseMode, maxDanger);
		}

		public bool CanReachNonLocal(IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			return dest.Map == this.map && this.CanReach(start, (LocalTargetInfo)dest, peMode, traverseParams);
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			return this.CanReach(start, dest, peMode, TraverseParms.For(traverseMode, maxDanger, false));
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			if (traverseParams.pawn != null)
			{
				if (!traverseParams.pawn.Spawned)
				{
					return false;
				}
				if (traverseParams.pawn.Map != this.map)
				{
					Log.Error(string.Concat(new object[]
					{
						"Called CanReach() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=",
						traverseParams.pawn,
						" pawn.Map=",
						traverseParams.pawn.Map,
						" map=",
						this.map
					}));
					return false;
				}
			}
			if (traverseParams.pawn != null && traverseParams.pawn.carryTracker != null && dest.HasThing && traverseParams.pawn.carryTracker.CarriedThing == dest.Thing)
			{
				return true;
			}
			if (dest.HasThing && dest.Thing.Map != this.map)
			{
				return false;
			}
			if (!start.InBounds(this.map) || !dest.Cell.InBounds(this.map))
			{
				return false;
			}
			if ((peMode == PathEndMode.OnCell || peMode == PathEndMode.Touch || peMode == PathEndMode.ClosestTouch) && RoomQuery.RoomAtFast(start, this.map) == RoomQuery.RoomAtFast(dest.Cell, this.map))
			{
				return true;
			}
			if (traverseParams.mode == TraverseMode.PassAnything)
			{
				return true;
			}
			if (this.working)
			{
				Log.ErrorOnce("Called ReachableBetween while working.", 7452341);
				return true;
			}
			dest = (LocalTargetInfo)GenPath.ResolvePathMode(dest.ToTargetInfo(this.map), ref peMode);
			this.working = true;
			this.pathGrid = this.map.pathGrid;
			this.regionGrid = this.map.regionGrid;
			this.reachedIndex += 1u;
			this.destRegions.Clear();
			if (peMode == PathEndMode.OnCell)
			{
				Region validRegionAt = this.regionGrid.GetValidRegionAt(dest.Cell);
				if (validRegionAt != null && validRegionAt.Allows(traverseParams, true))
				{
					this.destRegions.Add(validRegionAt);
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
						if (c.InBounds(this.map))
						{
							Region validRegionAt2 = this.regionGrid.GetValidRegionAt(c);
							if (validRegionAt2 != null && validRegionAt2.Allows(traverseParams, true))
							{
								this.destRegions.Add(validRegionAt2);
							}
						}
					}
				}
				else
				{
					this.neighList = GenAdjFast.AdjacentCells8Way(dest);
					for (int j = 0; j < this.neighList.Count; j++)
					{
						if (this.neighList[j].InBounds(this.map))
						{
							Region validRegionAt3 = this.regionGrid.GetValidRegionAt(this.neighList[j]);
							if (validRegionAt3 != null && validRegionAt3.Allows(traverseParams, true))
							{
								this.destRegions.Add(validRegionAt3);
							}
						}
					}
				}
			}
			if (this.destRegions.Count == 0)
			{
				this.FinalizeCheck();
				return false;
			}
			this.destRegions.RemoveDuplicates<Region>();
			this.openQueue.Clear();
			this.numRegionsOpened = 0;
			this.startingRegions.Clear();
			if (this.pathGrid.WalkableFast(start))
			{
				Region validRegionAt4 = this.regionGrid.GetValidRegionAt(start);
				this.QueueNewOpenRegion(validRegionAt4);
				this.startingRegions.Add(validRegionAt4);
			}
			else
			{
				for (int k = 0; k < 8; k++)
				{
					IntVec3 intVec = start + GenAdj.AdjacentCells[k];
					if (intVec.InBounds(this.map))
					{
						if (this.pathGrid.WalkableFast(intVec))
						{
							Region validRegionAt5 = this.regionGrid.GetValidRegionAt(intVec);
							if (validRegionAt5 != null && validRegionAt5.reachedIndex != this.reachedIndex)
							{
								this.QueueNewOpenRegion(validRegionAt5);
								this.startingRegions.Add(validRegionAt5);
							}
						}
					}
				}
				if (this.openQueue.Count == 0)
				{
					this.FinalizeCheck();
					return false;
				}
			}
			bool flag = false;
			for (int l = 0; l < this.startingRegions.Count; l++)
			{
				for (int m = 0; m < this.destRegions.Count; m++)
				{
					if (this.destRegions[m] == this.startingRegions[l])
					{
						this.FinalizeCheck();
						return true;
					}
					BoolUnknown boolUnknown = this.cache.CachedResultFor(this.startingRegions[l].Room, this.destRegions[m].Room, traverseParams);
					if (boolUnknown == BoolUnknown.True)
					{
						this.FinalizeCheck();
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
				this.FinalizeCheck();
				return false;
			}
			while (this.openQueue.Count > 0)
			{
				Region region = this.openQueue.Dequeue();
				for (int n = 0; n < region.links.Count; n++)
				{
					RegionLink regionLink = region.links[n];
					for (int num = 0; num < 2; num++)
					{
						Region region2 = regionLink.regions[num];
						if (region2 != null && region2.reachedIndex != this.reachedIndex)
						{
							if (region2.Allows(traverseParams, false))
							{
								if (this.destRegions.Contains(region2))
								{
									for (int num2 = 0; num2 < this.startingRegions.Count; num2++)
									{
										this.cache.AddCachedResult(this.startingRegions[num2].Room, region2.Room, traverseParams, true);
									}
									this.FinalizeCheck();
									return true;
								}
								this.QueueNewOpenRegion(region2);
							}
						}
					}
				}
			}
			for (int num3 = 0; num3 < this.startingRegions.Count; num3++)
			{
				for (int num4 = 0; num4 < this.destRegions.Count; num4++)
				{
					this.cache.AddCachedResult(this.startingRegions[num3].Room, this.destRegions[num4].Room, traverseParams, false);
				}
			}
			this.FinalizeCheck();
			return false;
		}

		public bool CanReachColony(IntVec3 c)
		{
			return this.CanReachFactionBase(c, Faction.OfPlayer);
		}

		public bool CanReachFactionBase(IntVec3 c, Faction factionBaseFaction)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return this.CanReach(c, MapGenerator.PlayerStartSpot, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false));
			}
			if (!c.Walkable(this.map))
			{
				return false;
			}
			Faction faction = this.map.ParentFaction ?? Faction.OfPlayer;
			List<Pawn> list = this.map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return true;
				}
			}
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			if (faction == Faction.OfPlayer)
			{
				List<Building> allBuildingsColonist = this.map.listerBuildings.allBuildingsColonist;
				for (int j = 0; j < allBuildingsColonist.Count; j++)
				{
					if (this.CanReach(c, allBuildingsColonist[j], PathEndMode.Touch, traverseParams))
					{
						return true;
					}
				}
			}
			else
			{
				List<Thing> list2 = this.map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k].Faction == faction && this.CanReach(c, list2[k], PathEndMode.Touch, traverseParams))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool CanReachMapEdge(IntVec3 c, TraverseParms traverseParms)
		{
			if (traverseParms.pawn != null)
			{
				if (!traverseParms.pawn.Spawned)
				{
					return false;
				}
				if (traverseParms.pawn.Map != this.map)
				{
					Log.Error(string.Concat(new object[]
					{
						"Called CanReachMapEdge() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=",
						traverseParms.pawn,
						" pawn.Map=",
						traverseParms.pawn.Map,
						" map=",
						this.map
					}));
					return false;
				}
			}
			Region region = c.GetRegion(this.map);
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
