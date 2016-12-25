using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_TakeWoundedGuest : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 vec;
			if (!RCellFinder.TryFindBestExitSpot(pawn, out vec, TraverseMode.ByPawn))
			{
				return null;
			}
			Pawn pawn2 = KidnapAIUtility.ReachableWoundedGuest(pawn);
			if (pawn2 == null)
			{
				return null;
			}
			return new Job(JobDefOf.Kidnap)
			{
				targetA = pawn2,
				targetB = vec,
				maxNumToCarry = 1
			};
		}
	}
}
