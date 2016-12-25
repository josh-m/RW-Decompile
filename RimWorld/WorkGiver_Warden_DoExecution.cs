using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DoExecution : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (pawn2.guest.interactionMode == PrisonerInteractionMode.Execution && pawn.CanReserve(t, 1))
			{
				return new Job(JobDefOf.PrisonerExecution, t);
			}
			return null;
		}
	}
}
