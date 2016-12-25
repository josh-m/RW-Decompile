using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SpectateDutySpectateRect : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			IntVec3 intVec;
			if (!SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, out intVec, duty.spectateRectAllowedSides, 1, null))
			{
				return null;
			}
			IntVec3 centerCell = duty.spectateRect.CenterCell;
			Building edifice = intVec.GetEdifice();
			if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable && pawn.CanReserve(edifice, 1))
			{
				return new Job(JobDefOf.SpectateCeremony, edifice, centerCell);
			}
			return new Job(JobDefOf.SpectateCeremony, intVec, centerCell);
		}
	}
}
