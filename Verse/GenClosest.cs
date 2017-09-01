using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class GenClosest
	{
		private const int DefaultLocalTraverseRegionsBeforeGlobal = 30;

		private static bool EarlyOutSearch(IntVec3 start, Map map, ThingRequest thingReq, IEnumerable<Thing> customGlobalSearchSet)
		{
			if (thingReq.group == ThingRequestGroup.Everything)
			{
				Log.Error("Cannot do ClosestThingReachable searching everything without restriction.");
				return true;
			}
			if (!start.InBounds(map))
			{
				Log.Error(string.Concat(new object[]
				{
					"Did FindClosestThing with start out of bounds (",
					start,
					"), thingReq=",
					thingReq
				}));
				return true;
			}
			return thingReq.group == ThingRequestGroup.Nothing || (customGlobalSearchSet == null && !thingReq.IsUndefined && map.listerThings.ThingsMatching(thingReq).Count == 0);
		}

		public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1, bool forceGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			ProfilerThreadCheck.BeginSample("ClosestThingReachable");
			if (searchRegionsMax > 0 && customGlobalSearchSet != null && !forceGlobalSearch)
			{
				Log.ErrorOnce("searchRegionsMax > 0 && customGlobalSearchSet != null && !forceGlobalSearch. customGlobalSearchSet will never be used.", 634984);
			}
			if (GenClosest.EarlyOutSearch(root, map, thingReq, customGlobalSearchSet))
			{
				ProfilerThreadCheck.EndSample();
				return null;
			}
			Thing thing = null;
			if (!thingReq.IsUndefined)
			{
				int maxRegions = (searchRegionsMax <= 0) ? 30 : searchRegionsMax;
				thing = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, searchRegionsMin, maxRegions, maxDistance, traversableRegionTypes, ignoreEntirelyForbiddenRegions);
			}
			if (thing == null && (searchRegionsMax < 0 || forceGlobalSearch))
			{
				if (traversableRegionTypes != RegionType.Set_Passable)
				{
					Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767);
				}
				Predicate<Thing> validator2 = (Thing t) => map.reachability.CanReach(root, t, peMode, traverseParams) && (validator == null || validator(t));
				IEnumerable<Thing> searchSet = customGlobalSearchSet ?? map.listerThings.ThingsMatching(thingReq);
				thing = GenClosest.ClosestThing_Global(root, searchSet, maxDistance, validator2);
			}
			ProfilerThreadCheck.EndSample();
			return thing;
		}

		public static Thing ClosestThing_Regionwise_ReachablePrioritized(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, int minRegions = 24, int maxRegions = 30)
		{
			if (GenClosest.EarlyOutSearch(root, map, thingReq, null))
			{
				return null;
			}
			if (maxRegions < minRegions)
			{
				Log.ErrorOnce("maxRegions < minRegions", 754343);
			}
			ProfilerThreadCheck.BeginSample("ClosestThing_Regionwise_ReachablePrioritized");
			Thing result = null;
			if (!thingReq.IsUndefined)
			{
				result = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance, RegionType.Set_Passable, false);
			}
			ProfilerThreadCheck.EndSample();
			return result;
		}

		public static Thing RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Thing> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
			{
				Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.");
				return null;
			}
			ProfilerThreadCheck.BeginSample("RegionwiseBFSWorker");
			Region region = root.GetRegion(map, traversableRegionTypes);
			if (region == null)
			{
				ProfilerThreadCheck.EndSample();
				return null;
			}
			float maxDistSquared = maxDistance * maxDistance;
			RegionEntryPredicate entryCondition = (Region from, Region to) => to.Allows(traverseParams, false) && (maxDistance > 5000f || to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared);
			Thing closestThing = null;
			float closestDistSquared = 9999999f;
			float bestPrio = -3.40282347E+38f;
			int regionsSeen = 0;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.portal == null && !r.Allows(traverseParams, true))
				{
					return false;
				}
				if (!ignoreEntirelyForbiddenRegions || !r.IsForbiddenEntirely(traverseParams.pawn))
				{
					List<Thing> list = r.ListerThings.ThingsMatching(req);
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = list[i];
						if (ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, peMode, traverseParams.pawn))
						{
							float num = (priorityGetter == null) ? 0f : priorityGetter(thing);
							if (num >= bestPrio)
							{
								float num2 = (float)(thing.Position - root).LengthHorizontalSquared;
								if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared && (validator == null || validator(thing)))
								{
									closestThing = thing;
									closestDistSquared = num2;
									bestPrio = num;
								}
							}
						}
					}
				}
				regionsSeen++;
				return regionsSeen >= minRegions && closestThing != null;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
			ProfilerThreadCheck.EndSample();
			return closestThing;
		}

		public static Thing ClosestThing_Global(IntVec3 center, IEnumerable searchSet, float maxDistance = 99999f, Predicate<Thing> validator = null)
		{
			ProfilerThreadCheck.BeginSample("ClosestThing_Global");
			if (searchSet == null)
			{
				return null;
			}
			float num = 2.14748365E+09f;
			Thing result = null;
			float num2 = maxDistance * maxDistance;
			foreach (Thing thing in searchSet)
			{
				float num3 = (float)(center - thing.Position).LengthHorizontalSquared;
				if (num3 < num && num3 <= num2)
				{
					ProfilerThreadCheck.BeginSample("validator");
					if (validator != null && !validator(thing))
					{
						ProfilerThreadCheck.EndSample();
					}
					else
					{
						ProfilerThreadCheck.EndSample();
						if (thing.Spawned)
						{
							result = thing;
							num = num3;
						}
					}
				}
			}
			ProfilerThreadCheck.EndSample();
			return result;
		}

		public static Thing ClosestThing_Global_Reachable(IntVec3 center, Map map, IEnumerable<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null)
		{
			ProfilerThreadCheck.BeginSample("ClosestThing_Global_Reachable");
			if (searchSet == null)
			{
				return null;
			}
			int num = 0;
			int num2 = 0;
			Thing result = null;
			float num3 = -3.40282347E+38f;
			float num4 = 0f;
			float num5 = maxDistance * maxDistance;
			float num6 = 2.14748365E+09f;
			foreach (Thing current in searchSet)
			{
				num2++;
				float num7 = (float)(center - current.Position).LengthHorizontalSquared;
				if (num7 <= num5)
				{
					if (priorityGetter != null)
					{
						num4 = priorityGetter(current);
						if (num4 < num3)
						{
							continue;
						}
					}
					if (num4 > num3 || num7 < num6)
					{
						if (map.reachability.CanReach(center, current, peMode, traverseParams))
						{
							if (current.Spawned)
							{
								if (validator == null || validator(current))
								{
									result = current;
									num6 = num7;
									num3 = num4;
									num++;
								}
							}
						}
					}
				}
			}
			ProfilerThreadCheck.BeginSample(string.Concat(new object[]
			{
				"changedCount: ",
				num,
				" scanCount: ",
				num2
			}));
			ProfilerThreadCheck.EndSample();
			ProfilerThreadCheck.EndSample();
			return result;
		}
	}
}
