using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FillFermentingBarrel : WorkGiver_Scanner
	{
		private static string TemperatureTrans;

		private static string NoWortTrans;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.FermentingBarrel);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public static void Reset()
		{
			WorkGiver_FillFermentingBarrel.TemperatureTrans = "BadTemperature".Translate().ToLower();
			WorkGiver_FillFermentingBarrel.NoWortTrans = "NoWort".Translate();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Building_FermentingBarrel building_FermentingBarrel = t as Building_FermentingBarrel;
			if (building_FermentingBarrel == null || building_FermentingBarrel.Fermented || building_FermentingBarrel.SpaceLeftForWort <= 0)
			{
				return false;
			}
			float temperature = building_FermentingBarrel.Position.GetTemperature(building_FermentingBarrel.Map);
			CompProperties_TemperatureRuinable compProperties = building_FermentingBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
			if (temperature < compProperties.minSafeTemperature + 2f || temperature > compProperties.maxSafeTemperature - 2f)
			{
				JobFailReason.Is(WorkGiver_FillFermentingBarrel.TemperatureTrans);
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (this.FindWort(pawn, building_FermentingBarrel) == null)
			{
				JobFailReason.Is(WorkGiver_FillFermentingBarrel.NoWortTrans);
				return false;
			}
			return !t.IsBurning();
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Building_FermentingBarrel building_FermentingBarrel = (Building_FermentingBarrel)t;
			Thing t2 = this.FindWort(pawn, building_FermentingBarrel);
			return new Job(JobDefOf.FillFermentingBarrel, t, t2)
			{
				count = building_FermentingBarrel.SpaceLeftForWort
			};
		}

		private Thing FindWort(Pawn pawn, Building_FermentingBarrel barrel)
		{
			Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Wort), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}
	}
}
