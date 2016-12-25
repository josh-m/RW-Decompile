using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_Binge : ThinkNode_JobGiver
	{
		protected bool IgnoreForbid(Pawn pawn)
		{
			return pawn.InMentalState;
		}

		protected abstract int IngestInterval(Pawn pawn);

		protected override Job TryGiveJob(Pawn pawn)
		{
			int num = Find.TickManager.TicksGame - pawn.mindState.lastIngestTick;
			if (num > this.IngestInterval(pawn))
			{
				Job job = this.IngestJob(pawn);
				if (job != null)
				{
					return job;
				}
			}
			return null;
		}

		private Job IngestJob(Pawn pawn)
		{
			Thing thing = this.BestIngestTarget(pawn);
			if (thing == null)
			{
				return null;
			}
			ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing);
			return new Job(JobDefOf.Ingest, thing)
			{
				count = finalIngestibleDef.ingestible.maxNumToIngestAtOnce,
				ignoreForbidden = this.IgnoreForbid(pawn),
				overeat = true
			};
		}

		protected abstract Thing BestIngestTarget(Pawn pawn);
	}
}
