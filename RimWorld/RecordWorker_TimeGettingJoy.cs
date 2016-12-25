using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RecordWorker_TimeGettingJoy : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			Job curJob = pawn.CurJob;
			return curJob != null && curJob.def.joyKind != null;
		}
	}
}
