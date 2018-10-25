using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class BillUtility
	{
		public static Bill Clipboard;

		public static void TryDrawIngredientSearchRadiusOnMap(this Bill bill, IntVec3 center)
		{
			if (bill.ingredientSearchRadius < GenRadial.MaxRadialPatternRadius)
			{
				GenDraw.DrawRadiusRing(center, bill.ingredientSearchRadius);
			}
		}

		public static Bill MakeNewBill(this RecipeDef recipe)
		{
			if (recipe.UsesUnfinishedThing)
			{
				return new Bill_ProductionWithUft(recipe);
			}
			return new Bill_Production(recipe);
		}

		[DebuggerHidden]
		public static IEnumerable<IBillGiver> GlobalBillGivers()
		{
			foreach (Map map in Find.Maps)
			{
				foreach (Thing thing in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)))
				{
					IBillGiver billgiver = thing as IBillGiver;
					if (billgiver == null)
					{
						Log.ErrorOnce("Found non-bill-giver tagged as PotentialBillGiver", 13389774, false);
					}
					else
					{
						yield return billgiver;
					}
				}
				foreach (Thing thing2 in map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing)))
				{
					IBillGiver billgiver2 = thing2.GetInnerIfMinified() as IBillGiver;
					if (billgiver2 != null)
					{
						yield return billgiver2;
					}
				}
			}
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				foreach (Thing thing3 in caravan.AllThings)
				{
					IBillGiver billgiver3 = thing3.GetInnerIfMinified() as IBillGiver;
					if (billgiver3 != null)
					{
						yield return billgiver3;
					}
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Bill> GlobalBills()
		{
			foreach (IBillGiver billgiver in BillUtility.GlobalBillGivers())
			{
				foreach (Bill bill in billgiver.BillStack)
				{
					yield return bill;
				}
			}
			if (BillUtility.Clipboard != null)
			{
				yield return BillUtility.Clipboard;
			}
		}

		public static void Notify_ZoneStockpileRemoved(Zone_Stockpile stockpile)
		{
			foreach (Bill current in BillUtility.GlobalBills())
			{
				current.ValidateSettings();
			}
		}

		public static void Notify_ColonistUnavailable(Pawn pawn)
		{
			try
			{
				foreach (Bill current in BillUtility.GlobalBills())
				{
					current.ValidateSettings();
				}
			}
			catch (Exception arg)
			{
				Log.Error("Could not notify bills: " + arg, false);
			}
		}

		public static WorkGiverDef GetWorkgiver(this IBillGiver billGiver)
		{
			Thing thing = billGiver as Thing;
			if (thing == null)
			{
				Log.ErrorOnce(string.Format("Attempting to get the workgiver for a non-Thing IBillGiver {0}", billGiver.ToString()), 96810282, false);
				return null;
			}
			List<WorkGiverDef> allDefsListForReading = DefDatabase<WorkGiverDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkGiverDef workGiverDef = allDefsListForReading[i];
				WorkGiver_DoBill workGiver_DoBill = workGiverDef.Worker as WorkGiver_DoBill;
				if (workGiver_DoBill != null)
				{
					if (workGiver_DoBill.ThingIsUsableBillGiver(thing))
					{
						return workGiverDef;
					}
				}
			}
			Log.ErrorOnce(string.Format("Can't find a WorkGiver for a BillGiver {0}", thing.ToString()), 57348705, false);
			return null;
		}
	}
}
