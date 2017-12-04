using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Flick : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

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

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Flick) == null)
			{
				return false;
			}
			LocalTargetInfo target = t;
			return pawn.CanReserve(target, 1, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Flick, t);
		}
	}
}
