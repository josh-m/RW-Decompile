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

		public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMax = -1, bool forceGlobalSearch = false)
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
				thing = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, 0, maxRegions, maxDistance);
			}
			if (thing == null && (searchRegionsMax < 0 || forceGlobalSearch))
			{
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
				result = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance);
			}
			ProfilerThreadCheck.EndSample();
			return result;
		}

		public static Thing RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Thing> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance)
		{
			if (traverseParams.mode == TraverseMode.PassAnything)
			{
				Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAnything. Use ClosestThingGlobal.");
				return null;
			}
			ProfilerThreadCheck.BeginSample("RegionwiseBFSWorker");
			Region rootReg = map.regionGrid.GetValidRegionAt(root);
			if (rootReg == null)
			{
				ProfilerThreadCheck.EndSample();
				return null;
			}
			float maxDistSquared = maxDistance * maxDistance;
			RegionEntryPredicate entryCondition = (Region from, Region to) => (from == rootReg || from.Allows(traverseParams, false)) && (to.Allows(traverseParams, false) || (to.portal != null && (peMode == PathEndMode.Touch || peMode == PathEndMode.ClosestTouch))) && (maxDistance > 5000f || to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared);
			Thing closestThing = null;
			float closestDistSquared = 99999f;
			List<Thing> thingList = null;
			float curPrio = 0f;
			float bestPrio = -3.40282347E+38f;
			int regionsSeen = 0;
			Thing t;
			float thingDistSquared;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.portal == null && !r.Allows(traverseParams, true))
				{
					return false;
				}
				thingList = r.ListerThings.ThingsMatching(req);
				for (int i = 0; i < thingList.Count; i++)
				{
					t = thingList[i];
					if (t.Spawned)
					{
						if (priorityGetter != null)
						{
							curPrio = priorityGetter(t);
						}
						if (curPrio >= bestPrio)
						{
							thingDistSquared = (t.Position - root).LengthHorizontalSquared;
							if ((curPrio > bestPrio || thingDistSquared < closestDistSquared) && thingDistSquared < maxDistSquared && (validator == null || validator(t)))
							{
								closestThing = t;
								closestDistSquared = thingDistSquared;
								bestPrio = curPrio;
							}
						}
					}
				}
				regionsSeen++;
				return regionsSeen >= minRegions && closestThing != null;
			};
			ProfilerThreadCheck.EndSample();
			RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, maxRegions);
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
				float lengthHorizontalSquared = (center - thing.Position).LengthHorizontalSquared;
				if (lengthHorizontalSquared < num && lengthHorizontalSquared <= num2)
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
							num = lengthHorizontalSquared;
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
				float lengthHorizontalSquared = (center - current.Position).LengthHorizontalSquared;
				if (lengthHorizontalSquared <= num5)
				{
					if (priorityGetter != null)
					{
						num4 = priorityGetter(current);
						if (num4 < num3)
						{
							continue;
						}
					}
					if (num4 > num3 || lengthHorizontalSquared < num6)
					{
						if (map.reachability.CanReach(center, current, peMode, traverseParams))
						{
							if (current.Spawned)
							{
								if (validator == null || validator(current))
								{
									result = current;
									num6 = lengthHorizontalSquared;
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
