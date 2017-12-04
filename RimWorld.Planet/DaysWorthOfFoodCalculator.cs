using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class DaysWorthOfFoodCalculator
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<ThingStackPart> tmpThingStackParts = new List<ThingStackPart>();

		public const float InfiniteDaysWorthOfFood = 1000f;

		private static List<float> tmpDaysWorthOfFoodPerPawn = new List<float>();

		private static List<bool> tmpAnyFoodLeftIngestibleByPawn = new List<bool>();

		private static List<ThingCount> tmpFood = new List<ThingCount>();

		private static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingCount> extraFood, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			if (!DaysWorthOfFoodCalculator.AnyNonLocalPlantsEatingPawn(pawns, assumeCanEatLocalPlants))
			{
				return 1000f;
			}
			DaysWorthOfFoodCalculator.tmpFood.Clear();
			if (extraFood != null)
			{
				for (int i = 0; i < extraFood.Count; i++)
				{
					if (extraFood[i].ThingDef.IsNutritionGivingIngestible && extraFood[i].Count > 0)
					{
						DaysWorthOfFoodCalculator.tmpFood.Add(extraFood[i]);
					}
				}
			}
			for (int j = 0; j < pawns.Count; j++)
			{
				if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawns[j], ignoreInventory))
				{
					ThingOwner<Thing> innerContainer = pawns[j].inventory.innerContainer;
					for (int k = 0; k < innerContainer.Count; k++)
					{
						if (innerContainer[k].def.IsNutritionGivingIngestible)
						{
							DaysWorthOfFoodCalculator.tmpFood.Add(new ThingCount(innerContainer[k].def, innerContainer[k].stackCount));
						}
					}
				}
			}
			if (!DaysWorthOfFoodCalculator.tmpFood.Any<ThingCount>())
			{
				return 0f;
			}
			DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn.Clear();
			DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn.Clear();
			for (int l = 0; l < pawns.Count; l++)
			{
				DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn.Add(0f);
				DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn.Add(true);
			}
			float num = 0f;
			bool flag;
			do
			{
				flag = false;
				for (int m = 0; m < pawns.Count; m++)
				{
					Pawn pawn = pawns[m];
					if (DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn[m])
					{
						if (pawn.RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn)))
						{
							do
							{
								int num2 = DaysWorthOfFoodCalculator.BestEverEdibleFoodIndexFor(pawns[m], DaysWorthOfFoodCalculator.tmpFood);
								if (num2 < 0)
								{
									goto Block_13;
								}
								float num3 = Mathf.Min(DaysWorthOfFoodCalculator.tmpFood[num2].ThingDef.ingestible.nutrition, pawn.needs.food.NutritionBetweenHungryAndFed);
								float num4 = num3 / pawn.needs.food.NutritionBetweenHungryAndFed * (float)pawn.needs.food.TicksUntilHungryWhenFed / 60000f;
								List<float> list;
								int index;
								(list = DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn)[index = m] = list[index] + num4;
								DaysWorthOfFoodCalculator.tmpFood[num2] = DaysWorthOfFoodCalculator.tmpFood[num2].WithCount(DaysWorthOfFoodCalculator.tmpFood[num2].Count - 1);
								flag = true;
							}
							while (DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[m] < num);
							IL_2CB:
							num = Mathf.Max(num, DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[m]);
							goto IL_2E0;
							Block_13:
							DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn[m] = false;
							goto IL_2CB;
						}
					}
					IL_2E0:;
				}
			}
			while (flag);
			float num5 = 1000f;
			for (int n = 0; n < pawns.Count; n++)
			{
				if (pawns[n].RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawns[n])))
				{
					num5 = Mathf.Min(num5, DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[n]);
				}
			}
			return num5;
		}

		public static float ApproxDaysWorthOfFood(Caravan caravan)
		{
			return DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(caravan.PawnsListForReading, null, VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(caravan.Tile), IgnorePawnsInventoryMode.DontIgnore);
		}

		public static float ApproxDaysWorthOfFood(List<TransferableOneWay> transferables, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing)
				{
					if (transferableOneWay.AnyThing is Pawn)
					{
						for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
						{
							Pawn pawn = (Pawn)transferableOneWay.things[j];
							if (pawn.RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn)))
							{
								DaysWorthOfFoodCalculator.tmpPawns.Add(pawn);
							}
						}
					}
					else
					{
						DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
					}
				}
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpPawns, DaysWorthOfFoodCalculator.tmpThingCounts, assumeCanEatLocalPlants, ignoreInventory);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTransfer(List<TransferableOneWay> transferables, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing)
				{
					if (transferableOneWay.AnyThing is Pawn)
					{
						for (int j = transferableOneWay.things.Count - 1; j >= transferableOneWay.CountToTransfer; j--)
						{
							Pawn pawn = (Pawn)transferableOneWay.things[j];
							if (pawn.RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn)))
							{
								DaysWorthOfFoodCalculator.tmpPawns.Add(pawn);
							}
						}
					}
					else
					{
						DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(transferableOneWay.ThingDef, transferableOneWay.MaxCount - transferableOneWay.CountToTransfer));
					}
				}
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpPawns, DaysWorthOfFoodCalculator.tmpThingCounts, assumeCanEatLocalPlants, ignoreInventory);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<Thing> potentiallyFood, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn)))
				{
					DaysWorthOfFoodCalculator.tmpPawns.Add(pawn);
				}
			}
			for (int j = 0; j < potentiallyFood.Count; j++)
			{
				DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(potentiallyFood[j].def, potentiallyFood[j].stackCount));
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpPawns, DaysWorthOfFoodCalculator.tmpThingCounts, assumeCanEatLocalPlants, ignoreInventory);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingStackPart> potentiallyFood, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(potentiallyFood[i].Thing.def, potentiallyFood[i].Count));
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(pawns, DaysWorthOfFoodCalculator.tmpThingCounts, assumeCanEatLocalPlants, ignoreInventory);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, bool assumeCanEatLocalPlants, IgnorePawnsInventoryMode ignoreInventory)
		{
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, DaysWorthOfFoodCalculator.tmpThingStackParts);
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			for (int i = DaysWorthOfFoodCalculator.tmpThingStackParts.Count - 1; i >= 0; i--)
			{
				Pawn pawn = DaysWorthOfFoodCalculator.tmpThingStackParts[i].Thing as Pawn;
				if (pawn != null)
				{
					if (pawn.RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn)))
					{
						DaysWorthOfFoodCalculator.tmpPawns.Add(pawn);
					}
				}
				else
				{
					DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(DaysWorthOfFoodCalculator.tmpThingStackParts[i].Thing.def, DaysWorthOfFoodCalculator.tmpThingStackParts[i].Count));
				}
			}
			DaysWorthOfFoodCalculator.tmpThingStackParts.Clear();
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpPawns, DaysWorthOfFoodCalculator.tmpThingCounts, assumeCanEatLocalPlants, ignoreInventory);
			DaysWorthOfFoodCalculator.tmpPawns.Clear();
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			return result;
		}

		private static bool AnyNonLocalPlantsEatingPawn(List<Pawn> pawns, bool assumeCanEatLocalPlants)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].RaceProps.EatsFood && (!assumeCanEatLocalPlants || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawns[i])))
				{
					return true;
				}
			}
			return false;
		}

		private static int BestEverEdibleFoodIndexFor(Pawn pawn, List<ThingCount> food)
		{
			int num = -1;
			float num2 = 0f;
			for (int i = 0; i < food.Count; i++)
			{
				if (food[i].Count > 0)
				{
					ThingDef thingDef = food[i].ThingDef;
					if (CaravanPawnsNeedsUtility.CanEverEatForNutrition(thingDef, pawn))
					{
						float foodScore = CaravanPawnsNeedsUtility.GetFoodScore(thingDef, pawn);
						if (num < 0 || foodScore > num2)
						{
							num = i;
							num2 = foodScore;
						}
					}
				}
			}
			return num;
		}
	}
}
