using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DeliverFood : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (!pawn2.guest.ShouldBeBroughtFood)
			{
				return null;
			}
			if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
			{
				return null;
			}
			if (WardenFeedUtility.ShouldBeFed(pawn2))
			{
				return null;
			}
			Thing thing;
			ThingDef def;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out def, false, true, false, false))
			{
				return null;
			}
			if (thing.GetRoom() == pawn2.GetRoom())
			{
				return null;
			}
			if (WorkGiver_Warden_DeliverFood.FoodAvailableInRoomTo(pawn2))
			{
				return null;
			}
			return new Job(JobDefOf.DeliverFood, thing, pawn2)
			{
				count = FoodUtility.WillIngestStackCountOf(pawn2, def),
				targetC = RCellFinder.SpotToChewStandingNear(pawn2, thing)
			};
		}

		private static bool FoodAvailableInRoomTo(Pawn prisoner)
		{
			if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverFood.NutritionAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
			{
				return true;
			}
			float num = 0f;
			float num2 = 0f;
			Room room = RoomQuery.RoomAt(prisoner);
			if (room == null)
			{
				return false;
			}
			for (int i = 0; i < room.RegionCount; i++)
			{
				Region region = room.Regions[i];
				List<Thing> list = region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing = list[j];
					if (thing.def.ingestible.preferability > FoodPreferability.NeverForNutrition)
					{
						num2 += WorkGiver_Warden_DeliverFood.NutritionAvailableForFrom(prisoner, thing);
					}
				}
				if (region.ListerThings.ThingsOfDef(ThingDefOf.NutrientPasteDispenser).Any<Thing>() && prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					return true;
				}
				List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int k = 0; k < list2.Count; k++)
				{
					Pawn pawn = list2[k] as Pawn;
					if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.RaceProps.WillAutomaticallyEat(pawn.carryTracker.CarriedThing)))
					{
						num += pawn.needs.food.NutritionWanted;
					}
				}
			}
			return num2 + 0.5f >= num;
		}

		private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
		{
			if (foodSource.def.IsNutritionGivingIngestible && p.RaceProps.WillAutomaticallyEat(foodSource))
			{
				return foodSource.def.ingestible.nutrition * (float)foodSource.stackCount;
			}
			if (p.RaceProps.ToolUser && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
				if (building_NutrientPasteDispenser != null && building_NutrientPasteDispenser.CanDispenseNow)
				{
					return 99999f;
				}
			}
			return 0f;
		}
	}
}
