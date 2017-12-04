using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_InducePrisonerToEscape : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = JailbreakerMentalStateUtility.FindPrisoner(pawn);
			if (pawn2 == null || !pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				return null;
			}
			return new Job(JobDefOf.InducePrisonerToEscape, pawn2)
			{
				interaction = InteractionDefOf.SparkJailbreak
			};
		}
	}
}
