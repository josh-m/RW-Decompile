using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class DaysUntilRotCalculator
	{
		private static List<Thing> tmpThings = new List<Thing>();

		private static List<ThingStackPart> tmpThingStackParts = new List<ThingStackPart>();

		public const float InfiniteDaysUntilRot = 1000f;

		public static float ApproxDaysUntilRot(List<Thing> potentiallyFood, int assumingTile)
		{
			float num = 1000f;
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				Thing thing = potentiallyFood[i];
				if (thing.def.IsNutritionGivingIngestible)
				{
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null && compRottable.Active)
					{
						num = Mathf.Min(num, (float)compRottable.ApproxTicksUntilRotWhenAtTempOfTile(assumingTile) / 60000f);
					}
				}
			}
			return num;
		}

		public static float ApproxDaysUntilRot(Caravan caravan)
		{
			return DaysUntilRotCalculator.ApproxDaysUntilRot(CaravanInventoryUtility.AllInventoryItems(caravan), caravan.Tile);
		}

		public static float ApproxDaysUntilRot(List<TransferableOneWay> transferables, int assumingTile, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysUntilRotCalculator.tmpThings.Clear();
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
							if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
							{
								ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
								for (int k = 0; k < innerContainer.Count; k++)
								{
									DaysUntilRotCalculator.tmpThings.Add(innerContainer[k]);
								}
							}
						}
					}
					else if (transferableOneWay.CountToTransfer > 0)
					{
						DaysUntilRotCalculator.tmpThings.AddRange(transferableOneWay.things);
					}
				}
			}
			float result = DaysUntilRotCalculator.ApproxDaysUntilRot(DaysUntilRotCalculator.tmpThings, assumingTile);
			DaysUntilRotCalculator.tmpThings.Clear();
			return result;
		}

		public static float ApproxDaysUntilRotLeftAfterTransfer(List<TransferableOneWay> transferables, int assumingTile, IgnorePawnsInventoryMode ignoreInventory)
		{
			DaysUntilRotCalculator.tmpThings.Clear();
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
							if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
							{
								ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
								for (int k = 0; k < innerContainer.Count; k++)
								{
									DaysUntilRotCalculator.tmpThings.Add(innerContainer[k]);
								}
							}
						}
					}
					else if (transferableOneWay.MaxCount - transferableOneWay.CountToTransfer > 0)
					{
						DaysUntilRotCalculator.tmpThings.AddRange(transferableOneWay.things);
					}
				}
			}
			float result = DaysUntilRotCalculator.ApproxDaysUntilRot(DaysUntilRotCalculator.tmpThings, assumingTile);
			DaysUntilRotCalculator.tmpThings.Clear();
			return result;
		}

		public static float ApproxDaysUntilRotLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, int assumingTile, IgnorePawnsInventoryMode ignoreInventory)
		{
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, DaysUntilRotCalculator.tmpThingStackParts);
			DaysUntilRotCalculator.tmpThings.Clear();
			for (int i = DaysUntilRotCalculator.tmpThingStackParts.Count - 1; i >= 0; i--)
			{
				if (DaysUntilRotCalculator.tmpThingStackParts[i].Count > 0)
				{
					Pawn pawn = DaysUntilRotCalculator.tmpThingStackParts[i].Thing as Pawn;
					if (pawn != null)
					{
						if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
						{
							ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
							for (int j = 0; j < innerContainer.Count; j++)
							{
								DaysUntilRotCalculator.tmpThings.Add(innerContainer[j]);
							}
						}
					}
					else
					{
						DaysUntilRotCalculator.tmpThings.Add(DaysUntilRotCalculator.tmpThingStackParts[i].Thing);
					}
				}
			}
			DaysUntilRotCalculator.tmpThingStackParts.Clear();
			float result = DaysUntilRotCalculator.ApproxDaysUntilRot(DaysUntilRotCalculator.tmpThings, assumingTile);
			DaysUntilRotCalculator.tmpThings.Clear();
			return result;
		}
	}
}
