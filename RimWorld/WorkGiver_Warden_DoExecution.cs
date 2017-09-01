using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DoExecution : WorkGiver_Warden
	{
		private static string IncapableOfViolenceLowerTrans;

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public static void Reset()
		{
			WorkGiver_Warden_DoExecution.IncapableOfViolenceLowerTrans = "IncapableOfViolenceLower".Translate();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (pawn2.guest.interactionMode != PrisonerInteractionModeDefOf.Execution || !pawn.CanReserve(t, 1, -1, null, false))
			{
				return null;
			}
			if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				JobFailReason.Is(WorkGiver_Warden_DoExecution.IncapableOfViolenceLowerTrans);
				return null;
			}
			return new Job(JobDefOf.PrisonerExecution, t);
		}
	}
}
