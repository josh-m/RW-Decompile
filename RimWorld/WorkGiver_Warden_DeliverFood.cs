using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DeliverFood : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (!pawn2.guest.CanBeBroughtFood)
			{
				return null;
			}
			if (!pawn2.Position.IsInPrisonCell(pawn2.Map))
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
			ThingDef thingDef;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false, true, false, false, false, false, false))
			{
				return null;
			}
			if (thing.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
			{
				return null;
			}
			if (WorkGiver_Warden_DeliverFood.FoodAvailableInRoomTo(pawn2))
			{
				return null;
			}
			float nutrition = FoodUtility.GetNutrition(thing, thingDef);
			return new Job(JobDefOf.DeliverFood, thing, pawn2)
			{
				count = FoodUtility.WillIngestStackCountOf(pawn2, thingDef, nutrition),
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
			Room room = prisoner.GetRoom(RegionType.Set_Passable);
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
					if (!thing.def.IsIngestible || thing.def.ingestible.preferability > FoodPreferability.DesperateOnlyForHumanlikes)
					{
						num2 += WorkGiver_Warden_DeliverFood.NutritionAvailableForFrom(prisoner, thing);
					}
				}
				List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int k = 0; k < list2.Count; k++)
				{
					Pawn pawn = list2[k] as Pawn;
					if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.WillEat(pawn.carryTracker.CarriedThing, null)))
					{
						num += pawn.needs.food.NutritionWanted;
					}
				}
			}
			return num2 + 0.5f >= num;
		}

		private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
		{
			if (foodSource.def.IsNutritionGivingIngestible && p.WillEat(foodSource, null))
			{
				return foodSource.GetStatValue(StatDefOf.Nutrition, true) * (float)foodSource.stackCount;
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
