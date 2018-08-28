using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SelfTend : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.RaceProps.Humanlike || !pawn.health.HasHediffsNeedingTend(false) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.InAggroMentalState)
			{
				return null;
			}
			if (pawn.IsColonist && pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
			{
				return null;
			}
			return new Job(JobDefOf.TendPatient, pawn)
			{
				endAfterTendedOnce = true
			};
		}
	}
}
