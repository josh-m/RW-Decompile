using RimWorld;
using System;

namespace Verse.AI
{
	public class JobGiver_ForcedGoto : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 forcedGotoPosition = pawn.mindState.forcedGotoPosition;
			if (!forcedGotoPosition.IsValid)
			{
				return null;
			}
			if (!pawn.CanReach(forcedGotoPosition, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
				return null;
			}
			return new Job(JobDefOf.Goto, forcedGotoPosition)
			{
				locomotionUrgency = LocomotionUrgency.Walk
			};
		}
	}
}
