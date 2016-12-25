using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_ReleasePrisoner : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (pawn2.guest.interactionMode != PrisonerInteractionMode.Release || pawn2.Downed || !pawn2.Awake())
			{
				return null;
			}
			IntVec3 vec;
			if (!RCellFinder.TryFindPrisonerReleaseCell(pawn2, pawn, out vec))
			{
				return null;
			}
			return new Job(JobDefOf.ReleasePrisoner, pawn2, vec)
			{
				maxNumToCarry = 1
			};
		}
	}
}
