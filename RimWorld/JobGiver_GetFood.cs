using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if (food.CurLevelPercentage < food.PercentageThreshHungry + 0.02f)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
			Thing thing;
			ThingDef def;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out def, true, true, false, true))
			{
				return null;
			}
			Pawn pawn2 = thing as Pawn;
			if (pawn2 != null)
			{
				return new Job(JobDefOf.PredatorHunt, pawn2)
				{
					killIncappedTarget = true
				};
			}
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
			{
				Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
				if (building != null)
				{
					ISlotGroupParent hopperSgp = building as ISlotGroupParent;
					Job job = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
					if (job != null)
					{
						return job;
					}
				}
				thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, FoodPreferability.MealLavish, false, false, false, false, false, false);
				if (thing == null)
				{
					return null;
				}
				def = thing.def;
			}
			return new Job(JobDefOf.Ingest, thing)
			{
				maxNumToCarry = FoodUtility.WillIngestStackCountOf(pawn, def)
			};
		}
	}
}
