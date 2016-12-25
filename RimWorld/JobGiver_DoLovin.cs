using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DoLovin : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
			{
				return null;
			}
			if (pawn.CurJob == null || !pawn.jobs.curDriver.layingDown || pawn.jobs.curDriver.layingDownBed == null || pawn.jobs.curDriver.layingDownBed.Medical || !pawn.health.capacities.CanBeAwake)
			{
				return null;
			}
			Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
			if (partnerInMyBed == null || !partnerInMyBed.health.capacities.CanBeAwake || Find.TickManager.TicksGame < partnerInMyBed.mindState.canLovinTick)
			{
				return null;
			}
			if (!pawn.CanReserve(partnerInMyBed, 1) || !partnerInMyBed.CanReserve(pawn, 1))
			{
				return null;
			}
			pawn.mindState.awokeVoluntarily = true;
			partnerInMyBed.mindState.awokeVoluntarily = true;
			return new Job(JobDefOf.Lovin, partnerInMyBed, pawn.jobs.curDriver.layingDownBed);
		}
	}
}
