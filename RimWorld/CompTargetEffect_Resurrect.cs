using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompTargetEffect_Resurrect : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (!user.IsColonistPlayerControlled)
			{
				return;
			}
			if (!user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
			{
				return;
			}
			Job job = new Job(JobDefOf.Resurrect, target, this.parent);
			job.count = 1;
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}
}
