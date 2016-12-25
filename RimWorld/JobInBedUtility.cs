using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class JobInBedUtility
	{
		public static void KeepLyingDown(this JobDriver driver, TargetIndex bedIndex)
		{
			driver.AddFinishAction(delegate
			{
				Pawn pawn = driver.pawn;
				if (!pawn.Drafted)
				{
					pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDefOf.LayDown, pawn.CurJob.GetTarget(bedIndex)));
				}
			});
		}
	}
}
