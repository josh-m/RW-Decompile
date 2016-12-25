using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PatientGoToBedEmergency : WorkGiver_PatientGoToBedRecuperate
	{
		public override Job NonScanJob(Pawn pawn)
		{
			if (!HealthAIUtility.ShouldBeTendedNowUrgent(pawn))
			{
				return null;
			}
			return base.NonScanJob(pawn);
		}
	}
}
