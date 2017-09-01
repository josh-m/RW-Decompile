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
			return (dest.Map == null || dest.Map == this.map) && this.CanReach(start, (LocalTargetInfo)dest, peMode, traverseMode, maxDanger);
		}

		public bool CanReachNonLocal(IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			return (dest.Map == null || dest.Map == this.map) && this.CanReach(start, (LocalTargetInfo)dest, peMode, traverseParams);
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			return this.CanReach(start, dest, peMode, TraverseParms.For(traverseMode, maxDanger, false));
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			if (this.working)
			{
				Log.ErrorOnce("Called ReachableBetween while working. This should never happen. Suppressing further errors.", 7312233);
				return false;
			}
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
			if (ReachabilityImmediate.CanReachImmediate(start, dest, this.map, peMode, traverseParams.pawn))
			{
				return true;
			}
			if (!dest.IsValid)
			{
				return false;
			}
			if (dest.HasThing && dest.Thing.Map != this.map)
			{
				return false;
			}
			if (!start.InBounds(this.map) || !dest.Cell.InBounds(this.map))
			{
				return false;
			}
			if (peMode == PathEndMode.OnCell || peMode == PathEndMode.Touch || peMode == PathEndMode.ClosestTouch)
			{
				Room room = RegionAndRoomQuery.RoomAtFast(start, this.map, RegionType.Set_Passable);
				if (room != null && room == RegionAndRoomQuery.RoomAtFast(dest.Cell, this.map, RegionType.Set_Passable))
				{
					return true;
				}
			}
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
			{
				TraverseParms traverseParams2 = traverseParams;
				traverseParams2.mode = TraverseMode.PassDoors;
				if (this.CanReach(start, dest, peMode, traverseParams2))
				{
					return true;
				}
			}
			dest = (LocalTargetInfo)GenPath.ResolvePathMode(traverseParams.pawn, dest.ToTargetInfo(this.map), ref peMode);
			this.working = true;
			bool result;
			try
			{
				this.pathGrid = this.map.pathGrid;
				this.regionGrid = this.map.regionGrid;
				this.reachedIndex += 1u;
				this.destRegions.Clear();
				if (peMode == PathEndMode.OnCell)
				{
					Region region = dest.Cell.GetRegion(this.map, RegionType.Set_Passable);
					if (region != null && region.Allows(traverseParams, true))
					{
						this.destRegions.Add(region);
					}
				}
				else if (peMode == PathEndMode.Touch)
				{
					TouchPathEndModeUtility.AddAllowedAdjacentRegions(dest, traverseParams, this.map, this.destRegions);
				}
				if (this.destRegions.Count == 0 && traverseParams.mode != TraverseMode.PassAllDestroyableThings)
				{
					this.FinalizeCheck();
					result = false;
				}
				else
				{
					this.destRegions.RemoveDuplicates<Region>();
					this.openQueue.Clear();
					this.numRegionsOpened = 0;
					this.DetermineStartRegions(start);
					if (this.openQueue.Count == 0 && traverseParams.mode != TraverseMode.PassAllDestroyableThings)
					{
						this.FinalizeCheck();
						result = false;
					}
					else
					{
						if (this.startingRegions.Any<Region>() && this.destRegions.Any<Region>())
						{
							switch (this.GetCachedResult(traverseParams))
							{
							case BoolUnknown.True:
								this.FinalizeCheck();
								result = true;
								return result;
							case BoolUnknown.False:
								this.FinalizeCheck();
								result = false;
								return result;
							}
						}
						if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
						{
							bool flag = this.CheckCellBasedReachability(start, dest, peMode, traverseParams);
							this.FinalizeCheck();
							result = flag;
						}
						else
						{
							bool flag2 = this.CheckRegionBasedReachability(traverseParams);
							this.FinalizeCheck();
							result = flag2;
						}
					}
				}
			}
			finally
			{
				this.working = false;
			}
			return result;
		}

		private void DetermineStartRegions(IntVec3 start)
		{
			this.startingRegions.Clear();
			if (this.pathGrid.WalkableFast(start))
			{
				Region validRegionAt = this.regionGrid.GetValidRegionAt(start);
				this.QueueNewOpenRegion(validRegionAt);
				this.startingRegions.Add(validRegionAt);
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = start + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(this.map))
					{
						if (this.pathGrid.WalkableFast(intVec))
						{
							Region validRegionAt2 = this.regionGrid.GetValidRegionAt(intVec);
							if (validRegionAt2 != null && validRegionAt2.reachedIndex != this.reachedIndex)
							{
								this.QueueNewOpenRegion(validRegionAt2);
								this.startingRegions.Add(validRegionAt2);
							}
						}
					}
				}
			}
		}

		private BoolUnknown GetCachedResult(TraverseParms traverseParams)
		{
			bool flag = false;
			for (int i = 0; i < this.startingRegions.Count; i++)
			{
				for (int j = 0; j < this.destRegions.Count; j++)
				{
					if (this.destRegions[j] == this.startingRegions[i])
					{
						return BoolUnknown.True;
					}
					BoolUnknown boolUnknown = this.cache.CachedResultFor(this.startingRegions[i].Room, this.destRegions[j].Room, traverseParams);
					if (boolUnknown == BoolUnknown.True)
					{
						return BoolUnknown.True;
					}
					if (boolUnknown == BoolUnknown.Unknown)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return BoolUnknown.False;
			}
			return BoolUnknown.Unknown;
		}

		private bool CheckRegionBasedReachability(TraverseParms traverseParams)
		{
			while (this.openQueue.Count > 0)
			{
				Region region = this.openQueue.Dequeue();
				for (int i = 0; i < region.links.Count; i++)
				{
					RegionLink regionLink = region.links[i];
					for (int j = 0; j < 2; j++)
					{
						Region region2 = regionLink.regions[j];
						if (region2 != null && region2.reachedIndex != this.reachedIndex && region2.type.Passable())
						{
							if (region2.Allows(traverseParams, false))
							{
								if (this.destRegions.Contains(region2))
								{
									for (int k = 0; k < this.startingRegions.Count; k++)
									{
										this.cache.AddCachedResult(this.startingRegions[k].Room, region2.Room, traverseParams, true);
									}
									return true;
								}
								this.QueueNewOpenRegion(region2);
							}
						}
					}
				}
			}
			for (int l = 0; l < this.startingRegions.Count; l++)
			{
				for (int m = 0; m < this.destRegions.Count; m++)
				{
					this.cache.AddCachedResult(this.startingRegions[l].Room, this.destRegions[m].Room, traverseParams, false);
				}
			}
			return false;
		}

		private bool CheckCellBasedReachability(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			IntVec3 foundCell = IntVec3.Invalid;
			Region[] directRegionGrid = this.regionGrid.DirectGrid;
			PathGrid pathGrid = this.map.pathGrid;
			CellIndices cellIndices = this.map.cellIndices;
			this.map.floodFiller.FloodFill(start, delegate(IntVec3 c)
			{
				int num = cellIndices.CellToIndex(c);
				if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
				{
					if (!pathGrid.WalkableFast(num))
					{
						Building edifice = c.GetEdifice(this.map);
						if (edifice == null || !PathFinder.IsDestroyable(edifice))
						{
							return false;
						}
					}
				}
				else
				{
					Log.ErrorOnce("Do not use this method for other modes than PassAllDestroyableThings!", 938476762);
					if (!pathGrid.WalkableFast(num))
					{
						return false;
					}
				}
				Region region = directRegionGrid[num];
				return region == null || region.Allows(traverseParams, false);
			}, delegate(IntVec3 c)
			{
				if (ReachabilityImmediate.CanReachImmediate(c, dest, this.map, peMode, traverseParams.pawn))
				{
					foundCell = c;
					return true;
				}
				return false;
			}, false);
			if (foundCell.IsValid)
			{
				Region validRegionAt = this.regionGrid.GetValidRegionAt(foundCell);
				if (validRegionAt != null)
				{
					for (int i = 0; i < this.startingRegions.Count; i++)
					{
						this.cache.AddCachedResult(this.startingRegions[i].Room, validRegionAt.Room, traverseParams, true);
					}
				}
				return true;
			}
			for (int j = 0; j < this.startingRegions.Count; j++)
			{
				for (int k = 0; k < this.destRegions.Count; k++)
				{
					this.cache.AddCachedResult(this.startingRegions[j].Room, this.destRegions[k].Room, traverseParams, false);
				}
			}
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
			Region region = c.GetRegion(this.map, RegionType.Set_Passable);
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
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, RegionType.Set_Passable);
			return foundReg;
		}

		public bool CanReachUnfogged(IntVec3 c, TraverseParms traverseParms)
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
						"Called CanReachUnfogged() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=",
						traverseParms.pawn,
						" pawn.Map=",
						traverseParms.pawn.Map,
						" map=",
						this.map
					}));
					return false;
				}
			}
			if (!c.InBounds(this.map))
			{
				return false;
			}
			if (!c.Fogged(this.map))
			{
				return true;
			}
			Region region = c.GetRegion(this.map, RegionType.Set_Passable);
			if (region == null)
			{
				return false;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
			bool foundReg = false;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (!r.AnyCell.Fogged(this.map))
				{
					foundReg = true;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, RegionType.Set_Passable);
			return foundReg;
		}
	}
}
