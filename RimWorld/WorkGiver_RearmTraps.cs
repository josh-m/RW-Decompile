using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RearmTraps : WorkGiver_Scanner
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
			foreach (Designation des in pawn.Map.designationManager.DesignationsOfDef(DesignationDefOf.RearmTrap))
			{
				yield return des.target.Thing;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.RearmTrap) == null)
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1))
			{
				return false;
			}
			List<Thing> thingList = t.Position.GetThingList(t.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				IntVec3 intVec;
				if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item && (thingList[i].IsForbidden(pawn) || !HaulAIUtility.CanHaulAside(pawn, thingList[i], out intVec)))
				{
					return false;
				}
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			List<Thing> thingList = t.Position.GetThingList(t.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item)
				{
					Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[i]);
					if (job != null)
					{
						return job;
					}
				}
			}
			return new Job(JobDefOf.RearmTrap, t);
		}
	}
}
