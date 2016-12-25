using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_TakeBeerOutOfFermentingBarrel : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.FermentingBarrel);
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
			Building_FermentingBarrel building_FermentingBarrel = t as Building_FermentingBarrel;
			return building_FermentingBarrel != null && building_FermentingBarrel.Fermented && !t.IsBurning() && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.TakeBeerOutOfFermentingBarrel, t);
		}
	}
}
