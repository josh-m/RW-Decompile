using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Flick : WorkGiver_Scanner
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Designation> desList = pawn.Map.designationManager.allDesignations;
			for (int i = 0; i < desList.Count; i++)
			{
				if (desList[i].def == DesignationDefOf.Flick)
				{
					yield return desList[i].target.Thing;
				}
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			return pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Flick) != null && pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.Flick, t);
		}
	}
}
