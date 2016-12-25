using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FixBrokenDownBuilding : WorkGiver_Scanner
	{
		public static string NotInHomeAreaTrans;

		private static string NoComponentsToRepairTrans;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public static void CacheTranslations()
		{
			WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans = "NotInHomeArea".Translate();
			WorkGiver_FixBrokenDownBuilding.NoComponentsToRepairTrans = "NoComponentsToRepair".Translate();
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.GetComponent<BreakdownManager>().brokenDownThings;
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return pawn.Map.GetComponent<BreakdownManager>().brokenDownThings.Count == 0;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (!building.def.building.repairable)
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!t.IsBrokenDown())
			{
				return false;
			}
			if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
			{
				JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
				return false;
			}
			if (!pawn.CanReserve(building, 1))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (building.IsBurning())
			{
				return false;
			}
			if (this.FindClosestComponent(pawn) == null)
			{
				JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NoComponentsToRepairTrans);
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Thing t2 = this.FindClosestComponent(pawn);
			return new Job(JobDefOf.FixBrokenDownBuilding, t, t2)
			{
				count = 1
			};
		}

		private Thing FindClosestComponent(Pawn pawn)
		{
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Component), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}
	}
}
