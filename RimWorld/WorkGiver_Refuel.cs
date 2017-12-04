using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Refuel : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Refuelable);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			if (compRefuelable == null || compRefuelable.IsFull)
			{
				return false;
			}
			bool flag = !forced;
			if (flag && !compRefuelable.ShouldAutoRefuelNow)
			{
				return false;
			}
			if (!t.IsForbidden(pawn))
			{
				LocalTargetInfo target = t;
				if (pawn.CanReserve(target, 1, -1, null, forced))
				{
					if (t.Faction != pawn.Faction)
					{
						return false;
					}
					ThingWithComps thingWithComps = t as ThingWithComps;
					if (thingWithComps != null)
					{
						CompFlickable comp = thingWithComps.GetComp<CompFlickable>();
						if (comp != null && !comp.SwitchIsOn)
						{
							return false;
						}
					}
					if (this.FindBestFuel(pawn, t) == null)
					{
						ThingFilter fuelFilter = t.TryGetComp<CompRefuelable>().Props.fuelFilter;
						JobFailReason.Is("NoFuelToRefuel".Translate(new object[]
						{
							fuelFilter.Summary
						}));
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing t2 = this.FindBestFuel(pawn, t);
			return new Job(JobDefOf.Refuel, t, t2);
		}

		private Thing FindBestFuel(Pawn pawn, Thing refuelable)
		{
			ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
			Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
			IntVec3 position = pawn.Position;
			Map map = pawn.Map;
			ThingRequest bestThingRequest = filter.BestThingRequest;
			PathEndMode peMode = PathEndMode.ClosestTouch;
			TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
		}
	}
}
