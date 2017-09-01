using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatheringsUtility
	{
		public static bool ShouldGuestKeepAttendingGathering(Pawn p)
		{
			return !p.Downed && (p.needs == null || !p.needs.food.Starving) && p.health.hediffSet.BleedRateTotal <= 0f && p.needs.rest.CurCategory < RestCategory.Exhausted && !p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff(false) && p.Awake() && !p.InAggroMentalState && !p.IsPrisoner;
		}

		public static bool AnyLordJobPreventsNewGatherings(Map map)
		{
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob lordJob = lords[i].LordJob;
				if (!lordJob.AllowStartNewGatherings)
				{
					return true;
				}
			}
			return false;
		}
	}
}
