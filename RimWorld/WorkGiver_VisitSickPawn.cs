using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_VisitSickPawn : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.InteractionCell;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return !InteractionUtility.CanInitiateInteraction(pawn);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && SickPawnVisitUtility.CanVisit(pawn, pawn2, JoyCategory.VeryLow);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			return new Job(JobDefOf.VisitSickPawn, pawn2, SickPawnVisitUtility.FindChair(pawn, pawn2))
			{
				ignoreJoyTimeAssignment = true
			};
		}
	}
}
