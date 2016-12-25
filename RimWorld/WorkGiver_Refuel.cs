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

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			return this.CanRefuel(pawn, t, true);
		}

		public override bool HasJobOnThingForced(Pawn pawn, Thing t)
		{
			return this.CanRefuel(pawn, t, false);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Thing t2 = this.FindBestFuel(pawn, t);
			return new Job(JobDefOf.Refuel, t, t2)
			{
				count = t.TryGetComp<CompRefuelable>().GetFuelCountToFullyRefuel()
			};
		}

		private bool CanRefuel(Pawn pawn, Thing t, bool mustBeAutoRefuelable)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			if (compRefuelable == null || compRefuelable.IsFull)
			{
				return false;
			}
			if (mustBeAutoRefuelable && !compRefuelable.ShouldAutoRefuelNow)
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1))
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

		private Thing FindBestFuel(Pawn pawn, Thing refuelable)
		{
			ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
			Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1) && filter.Allows(x);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}
	}
}
