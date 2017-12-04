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

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_FermentingBarrel building_FermentingBarrel = t as Building_FermentingBarrel;
			if (building_FermentingBarrel == null || !building_FermentingBarrel.Fermented)
			{
				return false;
			}
			if (t.IsBurning())
			{
				return false;
			}
			if (!t.IsForbidden(pawn))
			{
				LocalTargetInfo target = t;
				if (pawn.CanReserve(target, 1, -1, null, forced))
				{
					return true;
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.TakeBeerOutOfFermentingBarrel, t);
		}
	}
}
