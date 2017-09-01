using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_TendOther : WorkGiver_Tend
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return base.HasJobOnThing(pawn, t, forced) && pawn != t;
		}
	}
}
