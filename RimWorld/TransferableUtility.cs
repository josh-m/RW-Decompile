using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TransferableUtility
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public static void Transfer(List<Thing> things, int count, Action<Thing, Thing> transferred)
		{
			if (count <= 0)
			{
				return;
			}
			TransferableUtility.tmpThings.Clear();
			TransferableUtility.tmpThings.AddRange(things);
			int num = count;
			for (int i = 0; i < TransferableUtility.tmpThings.Count; i++)
			{
				Thing thing = TransferableUtility.tmpThings[i];
				int num2 = Mathf.Min(num, thing.stackCount);
				Thing thing2 = thing.SplitOff(num2);
				num -= num2;
				if (thing2 == thing)
				{
					things.Remove(thing);
				}
				transferred(thing2, thing);
				if (num <= 0)
				{
					break;
				}
			}
			TransferableUtility.tmpThings.Clear();
			if (num > 0)
			{
				Log.Error("Can't transfer things because there is nothing left.");
			}
		}

		public static void TransferNoSplit(List<Thing> things, int count, Action<Thing, int> transfer, bool removeIfTakingEntireThing = true, bool errorIfNotEnoughThings = true)
		{
			if (count <= 0)
			{
				return;
			}
			TransferableUtility.tmpThings.Clear();
			TransferableUtility.tmpThings.AddRange(things);
			int num = count;
			for (int i = 0; i < TransferableUtility.tmpThings.Count; i++)
			{
				Thing thing = TransferableUtility.tmpThings[i];
				int num2 = Mathf.Min(num, thing.stackCount);
				num -= num2;
				if (removeIfTakingEntireThing && num2 >= thing.stackCount)
				{
					things.Remove(thing);
				}
				transfer(thing, num2);
				if (num <= 0)
				{
					break;
				}
			}
			TransferableUtility.tmpThings.Clear();
			if (num > 0 && errorIfNotEnoughThings)
			{
				Log.Error("Can't transfer things because there is nothing left.");
			}
		}

		public static bool TransferAsOne(Thing a, Thing b)
		{
			if (a == b)
			{
				return true;
			}
			if (a.def.tradeNeverStack || b.def.tradeNeverStack)
			{
				return false;
			}
			if (a is Corpse && b is Corpse)
			{
				Pawn innerPawn = ((Corpse)a).InnerPawn;
				Pawn innerPawn2 = ((Corpse)b).InnerPawn;
				return innerPawn.def == innerPawn2.def && innerPawn.kindDef == innerPawn2.kindDef && !innerPawn.RaceProps.Humanlike && !innerPawn2.RaceProps.Humanlike && (innerPawn.Name == null || innerPawn.Name.Numerical) && (innerPawn2.Name == null || innerPawn2.Name.Numerical);
			}
			if (a.def.category == ThingCategory.Pawn)
			{
				if (b.def != a.def)
				{
					return false;
				}
				if (a.def.race.Humanlike || b.def.race.Humanlike)
				{
					return false;
				}
				Pawn pawn = (Pawn)a;
				Pawn pawn2 = (Pawn)b;
				return pawn.kindDef == pawn2.kindDef && pawn.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn2.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn.gender == pawn2.gender && (pawn.Name == null || pawn.Name.Numerical) && (pawn2.Name == null || pawn2.Name.Numerical) && pawn.ageTracker.CurLifeStageIndex == pawn2.ageTracker.CurLifeStageIndex && Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - pawn2.ageTracker.AgeBiologicalYearsFloat) <= 1f;
			}
			else
			{
				Apparel apparel = a as Apparel;
				Apparel apparel2 = b as Apparel;
				if (apparel != null && apparel2 != null && apparel.WornByCorpse != apparel2.WornByCorpse)
				{
					return false;
				}
				if (a.def.useHitPoints && Mathf.Abs(a.HitPoints - b.HitPoints) >= 10)
				{
					return false;
				}
				QualityCategory qualityCategory;
				QualityCategory qualityCategory2;
				if (a.TryGetQuality(out qualityCategory) && b.TryGetQuality(out qualityCategory2) && qualityCategory != qualityCategory2)
				{
					return false;
				}
				if (a.def.category == ThingCategory.Item)
				{
					return a.CanStackWith(b);
				}
				Log.Error(string.Concat(new object[]
				{
					"Unknown TransferAsOne pair: ",
					a,
					", ",
					b
				}));
				return false;
			}
		}

		public static T TransferableMatching<T>(Thing thing, List<T> transferables) where T : ITransferable
		{
			if (thing == null || transferables == null)
			{
				return default(T);
			}
			for (int i = 0; i < transferables.Count; i++)
			{
				T result = transferables[i];
				if (TransferableUtility.TransferAsOne(thing, result.AnyThing))
				{
					return result;
				}
			}
			return default(T);
		}

		public static List<Pawn> GetPawnsFromTransferables(List<TransferableOneWay> transferables)
		{
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].countToTransfer > 0)
				{
					if (transferables[i].AnyThing is Pawn)
					{
						for (int j = 0; j < transferables[i].countToTransfer; j++)
						{
							Pawn item = (Pawn)transferables[i].things[j];
							list.Add(item);
						}
					}
				}
			}
			return list;
		}

		public static void SimulateTradeableTransfer(List<Thing> all, List<Tradeable> tradeables, List<ThingStackPart> outThingsAfterTransfer)
		{
			outThingsAfterTransfer.Clear();
			for (int i = 0; i < all.Count; i++)
			{
				outThingsAfterTransfer.Add(new ThingStackPart(all[i], all[i].stackCount));
			}
			for (int j = 0; j < tradeables.Count; j++)
			{
				int countToTransfer = tradeables[j].CountToTransfer;
				int num = -countToTransfer;
				if (countToTransfer > 0)
				{
					TransferableUtility.TransferNoSplit(tradeables[j].thingsTrader, countToTransfer, delegate(Thing originalThing, int toTake)
					{
						outThingsAfterTransfer.Add(new ThingStackPart(originalThing, toTake));
					}, false, false);
				}
				else if (num > 0)
				{
					TransferableUtility.TransferNoSplit(tradeables[j].thingsColony, num, delegate(Thing originalThing, int toTake)
					{
						for (int k = 0; k < outThingsAfterTransfer.Count; k++)
						{
							ThingStackPart thingStackPart = outThingsAfterTransfer[k];
							if (thingStackPart.Thing == originalThing)
							{
								outThingsAfterTransfer[k] = thingStackPart.WithCount(thingStackPart.Count - toTake);
								break;
							}
						}
					}, false, false);
				}
			}
		}
	}
}
