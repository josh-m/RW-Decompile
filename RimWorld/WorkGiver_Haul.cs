using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Haul : WorkGiver_Scanner
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t, forced))
			{
				return null;
			}
			return HaulAIUtility.HaulToStorageJob(pawn, t);
		}
	}
}
