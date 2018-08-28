using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class RoofUtility
	{
		public static Thing FirstBlockingThing(IntVec3 pos, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(pos);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.plant != null && list[i].def.plant.interferesWithRoof)
				{
					return list[i];
				}
			}
			return null;
		}

		public static bool CanHandleBlockingThing(Thing blocker, Pawn worker, bool forced = false)
		{
			if (blocker == null)
			{
				return true;
			}
			if (blocker.def.category == ThingCategory.Plant)
			{
				LocalTargetInfo target = blocker;
				PathEndMode peMode = PathEndMode.ClosestTouch;
				Danger maxDanger = worker.NormalMaxDanger();
				if (worker.CanReserveAndReach(target, peMode, maxDanger, 1, -1, null, forced))
				{
					return true;
				}
			}
			return false;
		}

		public static Job HandleBlockingThingJob(Thing blocker, Pawn worker, bool forced = false)
		{
			if (blocker == null)
			{
				return null;
			}
			if (blocker.def.category == ThingCategory.Plant)
			{
				LocalTargetInfo target = blocker;
				PathEndMode peMode = PathEndMode.ClosestTouch;
				Danger maxDanger = worker.NormalMaxDanger();
				if (worker.CanReserveAndReach(target, peMode, maxDanger, 1, -1, null, forced))
				{
					return new Job(JobDefOf.CutPlant, blocker);
				}
			}
			return null;
		}
	}
}
