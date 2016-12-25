using RimWorld;
using System;

namespace Verse.AI
{
	public static class GenJob
	{
		public static void ValidateJob(Job job)
		{
			if (job.def == JobDefOf.HaulToContainer || job.def == JobDefOf.HaulToCell)
			{
				if (job.maxNumToCarry < 1)
				{
					Log.Error("Job has maxNumToHaul=" + job.maxNumToCarry);
					job.maxNumToCarry = 1;
				}
				if (job.haulMode == HaulMode.Undefined)
				{
					Log.Error("Job has haulMode=" + job.haulMode);
					job.haulMode = HaulMode.ToCellNonStorage;
				}
				if (job.haulMode == HaulMode.ToContainer && !(job.targetB.Thing is IThingContainerOwner))
				{
					Log.Error(string.Concat(new object[]
					{
						"Job has haulMode=",
						job.haulMode,
						" but targetB is not a ThingContainerGiver. It is ",
						job.targetB
					}));
					job.def = JobDefOf.Wait;
					job.expiryInterval = 30;
				}
			}
		}
	}
}
