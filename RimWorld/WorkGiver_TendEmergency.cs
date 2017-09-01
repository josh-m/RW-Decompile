using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_TendEmergency : WorkGiver_TendOther
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return base.HasJobOnThing(pawn, t, forced) && HealthAIUtility.ShouldBeTendedNowUrgent((Pawn)t);
		}
	}
}
