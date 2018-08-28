using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ReachOutside : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Room room = pawn.GetRoom(RegionType.Set_Passable);
			if (room.PsychologicallyOutdoors && room.TouchesMapEdge)
			{
				return null;
			}
			if (!pawn.CanReachMapEdge())
			{
				return null;
			}
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out intVec))
			{
				return null;
			}
			if (intVec == pawn.Position)
			{
				return null;
			}
			return new Job(JobDefOf.Goto, intVec);
		}
	}
}
