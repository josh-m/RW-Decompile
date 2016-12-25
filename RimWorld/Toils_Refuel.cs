using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Toils_Refuel
	{
		public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				Thing thing = curJob.GetTarget(refuelableInd).Thing;
				Thing thing2 = curJob.GetTarget(fuelInd).Thing;
				thing.TryGetComp<CompRefuelable>().Refuel(thing2);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
