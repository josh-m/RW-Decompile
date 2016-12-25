using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PatientGoToBed : ThinkNode
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (!pawn.health.PrefersMedicalRest)
			{
				return ThinkResult.NoJob;
			}
			if (RestUtility.TimetablePreventsLayDown(pawn) && !pawn.health.ShouldDoSurgeryNow && !pawn.health.ShouldBeTendedNow)
			{
				return ThinkResult.NoJob;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return ThinkResult.NoJob;
			}
			Thing thing = RestUtility.FindPatientBedFor(pawn);
			if (thing == null)
			{
				return ThinkResult.NoJob;
			}
			Job job = new Job(JobDefOf.LayDown, thing);
			return new ThinkResult(job, this);
		}
	}
}
