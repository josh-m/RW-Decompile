using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_TendEmergency : WorkGiver_Tend
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			return base.HasJobOnThing(pawn, t) && HealthAIUtility.ShouldBeTendedNowUrgent((Pawn)t);
		}
	}
}
