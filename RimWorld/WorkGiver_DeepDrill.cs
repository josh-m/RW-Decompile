using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_DeepDrill : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.DeepDrill);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.InteractionCell;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.DeepDrill).Cast<Thing>();
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == ThingDefOf.DeepDrill)
				{
					CompPowerTrader comp = allBuildingsColonist[i].GetComp<CompPowerTrader>();
					if (comp == null || comp.PowerOn)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (building.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(building, 1))
			{
				return false;
			}
			CompDeepDrill compDeepDrill = building.TryGetComp<CompDeepDrill>();
			return compDeepDrill.CanDrillNow() && !building.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.OperateDeepDrill, t, 1500, true);
		}
	}
}
