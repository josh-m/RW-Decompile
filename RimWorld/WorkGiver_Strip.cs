using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Strip : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Strip))
			{
				if (!des.target.HasThing)
				{
					Log.ErrorOnce("Strip designation has no target.", 63126);
				}
				else
				{
					yield return des.target.Thing;
				}
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			return Find.DesignationManager.DesignationOn(t, DesignationDefOf.Strip) != null && pawn.CanReserve(t, 1) && StrippableUtility.CanBeStrippedByColony(t);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.Strip, t);
		}
	}
}
