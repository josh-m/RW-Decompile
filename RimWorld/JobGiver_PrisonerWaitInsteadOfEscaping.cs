using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PrisonerWaitInsteadOfEscaping : JobGiver_Wander
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.guest == null || !pawn.guest.ShouldWaitInsteadOfEscaping)
			{
				return null;
			}
			Room room = pawn.GetRoom(RegionType.Set_Passable);
			if (room != null && room.isPrisonCell)
			{
				return null;
			}
			IntVec3 spotToWaitInsteadOfEscaping = pawn.guest.spotToWaitInsteadOfEscaping;
			if (!spotToWaitInsteadOfEscaping.IsValid || !pawn.CanReach(spotToWaitInsteadOfEscaping, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out spotToWaitInsteadOfEscaping))
				{
					return null;
				}
				pawn.guest.spotToWaitInsteadOfEscaping = spotToWaitInsteadOfEscaping;
			}
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.guest.spotToWaitInsteadOfEscaping;
		}
	}
}
