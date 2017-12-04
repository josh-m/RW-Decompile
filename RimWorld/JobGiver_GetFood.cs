using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		private HungerCategory minCategory;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetFood jobGiver_GetFood = (JobGiver_GetFood)base.DeepCopy(resolve);
			jobGiver_GetFood.minCategory = this.minCategory;
			return jobGiver_GetFood;
		}

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
			if (food.CurCategory < this.minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null || food.CurCategory < this.minCategory)
			{
				return null;
			}
			bool flag;
			if (pawn.AnimalOrWildMan())
			{
				flag = true;
			}
			else
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
				flag = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
			}
			bool flag2 = pawn.needs.food.CurCategory == HungerCategory.Starving;
			bool desperate = flag2;
			bool canRefillDispenser = true;
			bool canUseInventory = true;
			bool allowCorpse = flag;
			Thing thing;
			ThingDef thingDef;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out thingDef, canRefillDispenser, canUseInventory, false, allowCorpse, false, pawn.IsWildMan()))
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
			if (thing is Plant && thing.def.plant.harvestedThingDef == thingDef)
			{
				return new Job(JobDefOf.Harvest, thing);
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
				thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, flag2, out thingDef, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false, false);
				if (thing == null)
				{
					return null;
				}
			}
			return new Job(JobDefOf.Ingest, thing)
			{
				count = FoodUtility.WillIngestStackCountOf(pawn, thingDef)
			};
		}
	}
}
