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
			foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Strip))
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

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Map.designationManager.DesignationOn(t, DesignationDefOf.Strip) == null)
			{
				return false;
			}
			LocalTargetInfo target = t;
			return pawn.CanReserve(target, 1, -1, null, forced) && StrippableUtility.CanBeStrippedByColony(t);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Strip, t);
		}
	}
}
