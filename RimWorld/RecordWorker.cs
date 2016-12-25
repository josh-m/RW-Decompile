using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RecordWorker
	{
		public RecordDef def;

		public virtual bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (this.def.measuredTimeJobs == null)
			{
				return false;
			}
			Job curJob = pawn.CurJob;
			if (curJob == null)
			{
				return false;
			}
			for (int i = 0; i < this.def.measuredTimeJobs.Count; i++)
			{
				if (curJob.def == this.def.measuredTimeJobs[i])
				{
					return true;
				}
			}
			return false;
		}
	}
}
