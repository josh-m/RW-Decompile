using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanPawnsNeedsUtility
	{
		private const float AutoRefillMiscNeedsIfBelowLevel = 0.3f;

		private static List<Thing> tmpInvFood = new List<Thing>();

		public static void TrySatisfyPawnsNeeds(Caravan caravan)
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = pawnsListForReading.Count - 1; i >= 0; i--)
			{
				CaravanPawnsNeedsUtility.TrySatisfyPawnNeeds(pawnsListForReading[i], caravan);
			}
		}

		public static bool AnyPawnOutOfFood(Caravan c, out string malnutritionHediff)
		{
			CaravanPawnsNeedsUtility.tmpInvFood.Clear();
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.IsNutritionGivingIngestible)
				{
					CaravanPawnsNeedsUtility.tmpInvFood.Add(list[i]);
				}
			}
			List<Pawn> pawnsListForReading = c.PawnsListForReading;
			for (int j = 0; j < pawnsListForReading.Count; j++)
			{
				Pawn pawn = pawnsListForReading[j];
				if (pawn.RaceProps.EatsFood && !VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
				{
					bool flag = false;
					for (int k = 0; k < CaravanPawnsNeedsUtility.tmpInvFood.Count; k++)
					{
						if (CaravanPawnsNeedsUtility.CanEverEatForNutrition(CaravanPawnsNeedsUtility.tmpInvFood[k].def, pawn))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int num = -1;
						string text = null;
						for (int l = 0; l < pawnsListForReading.Count; l++)
						{
							Hediff firstHediffOfDef = pawnsListForReading[l].health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
							if (firstHediffOfDef != null && (text == null || firstHediffOfDef.CurStageIndex > num))
							{
								num = firstHediffOfDef.CurStageIndex;
								text = firstHediffOfDef.LabelCap;
							}
						}
						malnutritionHediff = text;
						return true;
					}
				}
			}
			malnutritionHediff = null;
			return false;
		}

		private static void TrySatisfyPawnNeeds(Pawn pawn, Caravan caravan)
		{
			if (pawn.Dead)
			{
				return;
			}
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need need = allNeeds[i];
				Need_Rest need_Rest = need as Need_Rest;
				Need_Food need_Food = need as Need_Food;
				Need_Chemical need_Chemical = need as Need_Chemical;
				if (need_Rest != null)
				{
					CaravanPawnsNeedsUtility.TrySatisfyRestNeed(pawn, need_Rest, caravan);
				}
				else if (need_Food != null)
				{
					CaravanPawnsNeedsUtility.TrySatisfyFoodNeed(pawn, need_Food, caravan);
				}
				else if (need_Chemical != null)
				{
					CaravanPawnsNeedsUtility.TrySatisfyChemicalNeed(pawn, need_Chemical, caravan);
				}
			}
		}

		private static void TrySatisfyRestNeed(Pawn pawn, Need_Rest rest, Caravan caravan)
		{
			if (caravan.Resting)
			{
				float restEffectiveness = RestUtility.PawnHealthRestEffectivenessFactor(pawn);
				rest.TickResting(restEffectiveness);
			}
		}

		private static void TrySatisfyFoodNeed(Pawn pawn, Need_Food food, Caravan caravan)
		{
			if (food.CurCategory < HungerCategory.Hungry)
			{
				return;
			}
			Thing thing;
			Pawn pawn2;
			if (VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
			{
				VirtualPlantsUtility.EatVirtualPlants(pawn);
			}
			else if (CaravanInventoryUtility.TryGetBestFood(caravan, pawn, out thing, out pawn2))
			{
				food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
				if (thing.Destroyed)
				{
					if (pawn2 != null)
					{
						pawn2.inventory.innerContainer.Remove(thing);
						caravan.RecacheImmobilizedNow();
						caravan.RecacheDaysWorthOfFood();
					}
					if (!CaravanInventoryUtility.TryGetBestFood(caravan, pawn, out thing, out pawn2))
					{
						Messages.Message("MessageCaravanRunOutOfFood".Translate(new object[]
						{
							caravan.LabelCap,
							pawn.Label
						}), caravan, MessageSound.SeriousAlert);
					}
				}
			}
		}

		private static void TrySatisfyChemicalNeed(Pawn pawn, Need_Chemical chemical, Caravan caravan)
		{
			if (chemical.CurCategory >= DrugDesireCategory.Satisfied)
			{
				return;
			}
			Thing drug;
			Pawn drugOwner;
			if (CaravanInventoryUtility.TryGetBestDrug(caravan, pawn, chemical, out drug, out drugOwner))
			{
				CaravanPawnsNeedsUtility.IngestDrug(pawn, drug, drugOwner, caravan);
			}
		}

		public static void IngestDrug(Pawn pawn, Thing drug, Pawn drugOwner, Caravan caravan)
		{
			float num = drug.Ingested(pawn, 0f);
			Need_Food food = pawn.needs.food;
			if (food != null)
			{
				food.CurLevel += num;
			}
			if (drug.Destroyed && drugOwner != null)
			{
				drugOwner.inventory.innerContainer.Remove(drug);
				caravan.RecacheImmobilizedNow();
				caravan.RecacheDaysWorthOfFood();
			}
		}

		public static bool CanEverEatForNutrition(ThingDef food, Pawn pawn)
		{
			return food.IsNutritionGivingIngestible && pawn.RaceProps.CanEverEat(food) && food.ingestible.preferability > FoodPreferability.NeverForNutrition && (!pawn.IsTeetotaler() || !food.IsDrug);
		}

		public static bool CanNowEatForNutrition(ThingDef food, Pawn pawn)
		{
			return CaravanPawnsNeedsUtility.CanEverEatForNutrition(food, pawn) && (pawn.needs.food.CurCategory >= HungerCategory.Starving || food.ingestible.preferability > FoodPreferability.DesperateOnly);
		}

		public static bool CanNowEatForNutrition(Thing food, Pawn pawn)
		{
			return food.IngestibleNow && CaravanPawnsNeedsUtility.CanNowEatForNutrition(food.def, pawn);
		}

		public static float GetFoodScore(Thing food, Pawn pawn)
		{
			float num = CaravanPawnsNeedsUtility.GetFoodScore(food.def, pawn);
			if (pawn.RaceProps.Humanlike)
			{
				CompRottable compRottable = food.TryGetComp<CompRottable>();
				int a = (compRottable == null) ? 2147483647 : compRottable.TicksUntilRotAtCurrentTemp;
				float a2 = 1f - (float)Mathf.Min(a, 3600000) / 3600000f;
				num += Mathf.Min(a2, 0.999f);
			}
			return num;
		}

		public static float GetFoodScore(ThingDef food, Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike)
			{
				return (float)food.ingestible.preferability;
			}
			float num = 0f;
			if (food == ThingDefOf.Kibble || food == ThingDefOf.Hay)
			{
				num = 4f;
			}
			else if (food.ingestible.preferability == FoodPreferability.RawBad)
			{
				num = 3f;
			}
			else if (food.ingestible.preferability == FoodPreferability.RawTasty)
			{
				num = 2f;
			}
			else if (food.ingestible.preferability < FoodPreferability.MealAwful)
			{
				num = 1f;
			}
			return num + Mathf.Min(food.ingestible.nutrition / 100f, 0.999f);
		}
	}
}
