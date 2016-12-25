using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_GoForWalk : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn, null))
			{
				return null;
			}
			if (PawnUtility.WillSoonHaveBasicNeed(pawn))
			{
				return null;
			}
			Region reg;
			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(pawn), 100, out reg))
			{
				return null;
			}
			IntVec3 root;
			if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, null, out root))
			{
				return null;
			}
			List<IntVec3> list;
			if (!WalkPathFinder.TryFindWalkPath(pawn, root, out list))
			{
				return null;
			}
			Job job = new Job(this.def.jobDef, list[0]);
			job.targetQueueA = new List<LocalTargetInfo>();
			for (int i = 1; i < list.Count; i++)
			{
				job.targetQueueA.Add(list[i]);
			}
			job.locomotionUrgency = LocomotionUrgency.Walk;
			return job;
		}
	}
}
