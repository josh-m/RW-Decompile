using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Open : WorkGiver_Scanner
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
			foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Open))
			{
				yield return des.target.Thing;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Open) == null)
			{
				return false;
			}
			LocalTargetInfo target = t;
			return pawn.CanReserve(target, 1, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.Open, t);
		}
	}
}
