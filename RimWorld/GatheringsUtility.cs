using System;
using Verse;

namespace RimWorld
{
	public static class GatheringsUtility
	{
		public static bool ShouldGuestKeepAttendingGathering(Pawn p)
		{
			return !p.Downed && !p.needs.food.Starving && p.health.hediffSet.BleedRateTotal <= 0f && p.needs.rest.CurCategory < RestCategory.Exhausted && !p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff(false) && p.Awake() && !p.InAggroMentalState && !p.IsPrisoner;
		}
	}
}
