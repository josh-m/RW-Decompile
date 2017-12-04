using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PlantsCut : WorkGiver_Scanner
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
				Designation des = desList[i];
				if (des.def == DesignationDefOf.CutPlant || des.def == DesignationDefOf.HarvestPlant)
				{
					yield return des.target.Thing;
				}
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.def.category != ThingCategory.Plant)
			{
				return null;
			}
			LocalTargetInfo target = t;
			if (!pawn.CanReserve(target, 1, -1, null, forced))
			{
				return null;
			}
			if (t.IsForbidden(pawn))
			{
				return null;
			}
			if (t.IsBurning())
			{
				return null;
			}
			foreach (Designation current in pawn.Map.designationManager.AllDesignationsOn(t))
			{
				if (current.def == DesignationDefOf.HarvestPlant)
				{
					Job result;
					if (!((Plant)t).HarvestableNow)
					{
						result = null;
						return result;
					}
					result = new Job(JobDefOf.Harvest, t);
					return result;
				}
				else if (current.def == DesignationDefOf.CutPlant)
				{
					Job result = new Job(JobDefOf.CutPlant, t);
					return result;
				}
			}
			return null;
		}
	}
}
