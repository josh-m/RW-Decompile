using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class DaysWorthOfFoodCalculator
	{
		public const float InfiniteDaysWorthOfFood = 1000f;

		private static List<Pawn> tmpNonGrassEatingPawns = new List<Pawn>();

		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<ThingStackPart> tmpThingStackParts = new List<ThingStackPart>();

		private static List<float> tmpDaysWorthOfFoodPerPawn = new List<float>();

		private static List<bool> tmpAnyFoodLeftIngestibleByPawn = new List<bool>();

		private static List<ThingCount> tmpFood = new List<ThingCount>();

		private static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingCount> potentiallyFood)
		{
			if (!DaysWorthOfFoodCalculator.AnyNonGrassEatingPawn(pawns))
			{
				return 1000f;
			}
			DaysWorthOfFoodCalculator.tmpFood.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				if (potentiallyFood[i].ThingDef.IsNutritionGivingIngestible && potentiallyFood[i].Count > 0)
				{
					DaysWorthOfFoodCalculator.tmpFood.Add(potentiallyFood[i]);
				}
			}
			if (!DaysWorthOfFoodCalculator.tmpFood.Any<ThingCount>())
			{
				return 0f;
			}
			DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn.Clear();
			DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn.Clear();
			for (int j = 0; j < pawns.Count; j++)
			{
				DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn.Add(0f);
				DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn.Add(true);
			}
			float num = 0f;
			bool flag;
			do
			{
				flag = false;
				for (int k = 0; k < pawns.Count; k++)
				{
					Pawn pawn = pawns[k];
					if (DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn[k])
					{
						if (!pawn.RaceProps.Eats(FoodTypeFlags.Plant))
						{
							do
							{
								int num2 = DaysWorthOfFoodCalculator.BestEverEdibleFoodIndexFor(pawns[k], DaysWorthOfFoodCalculator.tmpFood);
								if (num2 < 0)
								{
									goto Block_9;
								}
								float num3 = Mathf.Min(DaysWorthOfFoodCalculator.tmpFood[num2].ThingDef.ingestible.nutrition, pawn.needs.food.NutritionBetweenHungryAndFed);
								float num4 = num3 / pawn.needs.food.NutritionBetweenHungryAndFed * (float)pawn.needs.food.TicksUntilHungryWhenFed / 60000f;
								List<float> list;
								List<float> expr_1AE = list = DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn;
								int index;
								int expr_1B3 = index = k;
								float num5 = list[index];
								expr_1AE[expr_1B3] = num5 + num4;
								DaysWorthOfFoodCalculator.tmpFood[num2] = DaysWorthOfFoodCalculator.tmpFood[num2].WithCount(DaysWorthOfFoodCalculator.tmpFood[num2].Count - 1);
								flag = true;
							}
							while (DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[k] < num);
							IL_217:
							num = Mathf.Max(num, DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[k]);
							goto IL_22A;
							Block_9:
							DaysWorthOfFoodCalculator.tmpAnyFoodLeftIngestibleByPawn[k] = false;
							goto IL_217;
						}
					}
					IL_22A:;
				}
			}
			while (flag);
			float num6 = 1000f;
			for (int l = 0; l < pawns.Count; l++)
			{
				if (!pawns[l].RaceProps.Eats(FoodTypeFlags.Plant))
				{
					num6 = Mathf.Min(num6, DaysWorthOfFoodCalculator.tmpDaysWorthOfFoodPerPawn[l]);
				}
			}
			return num6;
		}

		public static float ApproxDaysWorthOfFood(Caravan caravan)
		{
			return DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(caravan.PawnsListForReading, CaravanInventoryUtility.AllInventoryItems(caravan));
		}

		public static float ApproxDaysWorthOfFood(List<TransferableOneWay> transferables)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing)
				{
					if (transferableOneWay.AnyThing is Pawn)
					{
						for (int j = 0; j < transferableOneWay.countToTransfer; j++)
						{
							Pawn pawn = (Pawn)transferableOneWay.things[j];
							if (!pawn.RaceProps.Eats(FoodTypeFlags.Plant))
							{
								DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Add(pawn);
							}
						}
					}
					else
					{
						DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
					}
				}
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns, DaysWorthOfFoodCalculator.tmpThingCounts);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTransfer(List<TransferableOneWay> transferables)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing)
				{
					if (transferableOneWay.AnyThing is Pawn)
					{
						for (int j = transferableOneWay.things.Count - 1; j >= transferableOneWay.countToTransfer; j--)
						{
							Pawn pawn = (Pawn)transferableOneWay.things[j];
							if (!pawn.RaceProps.Eats(FoodTypeFlags.Plant))
							{
								DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Add(pawn);
							}
						}
					}
					else
					{
						DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(transferableOneWay.ThingDef, transferableOneWay.MaxCount - transferableOneWay.CountToTransfer));
					}
				}
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns, DaysWorthOfFoodCalculator.tmpThingCounts);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<Thing> potentiallyFood)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (!pawn.RaceProps.Eats(FoodTypeFlags.Plant))
				{
					DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Add(pawn);
				}
			}
			for (int j = 0; j < potentiallyFood.Count; j++)
			{
				DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(potentiallyFood[j].def, potentiallyFood[j].stackCount));
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns, DaysWorthOfFoodCalculator.tmpThingCounts);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingStackPart> potentiallyFood)
		{
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(potentiallyFood[i].Thing.def, potentiallyFood[i].Count));
			}
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(pawns, DaysWorthOfFoodCalculator.tmpThingCounts);
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables)
		{
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, DaysWorthOfFoodCalculator.tmpThingStackParts);
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			for (int i = DaysWorthOfFoodCalculator.tmpThingStackParts.Count - 1; i >= 0; i--)
			{
				Pawn pawn = DaysWorthOfFoodCalculator.tmpThingStackParts[i].Thing as Pawn;
				if (pawn != null)
				{
					if (!pawn.RaceProps.Eats(FoodTypeFlags.Plant))
					{
						DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Add(pawn);
					}
				}
				else
				{
					DaysWorthOfFoodCalculator.tmpThingCounts.Add(new ThingCount(DaysWorthOfFoodCalculator.tmpThingStackParts[i].Thing.def, DaysWorthOfFoodCalculator.tmpThingStackParts[i].Count));
				}
			}
			DaysWorthOfFoodCalculator.tmpThingStackParts.Clear();
			float result = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns, DaysWorthOfFoodCalculator.tmpThingCounts);
			DaysWorthOfFoodCalculator.tmpNonGrassEatingPawns.Clear();
			DaysWorthOfFoodCalculator.tmpThingCounts.Clear();
			return result;
		}

		private static bool AnyNonGrassEatingPawn(List<Pawn> pawns)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].RaceProps.Eats(FoodTypeFlags.Plant))
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
