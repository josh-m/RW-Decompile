using System;
using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeHauling : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return !pawn.Dead && pawn.carryTracker.CarriedThing != null;
		}
	}
}
