using System;
using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeInBedForMedicalReasons : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.InBed() && (HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) || (HealthAIUtility.ShouldSeekMedicalRest(pawn) && (pawn.needs.rest.CurLevel >= 1f || pawn.CurJob.restUntilHealed)));
		}
	}
}
