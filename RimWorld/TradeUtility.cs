using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class TradeUtility
	{
		public static IEnumerable<Thing> AllLaunchableThings
		{
			get
			{
				HashSet<Thing> yieldedThings = new HashSet<Thing>();
				foreach (Building_OrbitalTradeBeacon beacon in Building_OrbitalTradeBeacon.AllPowered())
				{
					foreach (IntVec3 c in beacon.TradeableCells)
					{
						List<Thing> thingList = c.GetThingList();
						for (int i = 0; i < thingList.Count; i++)
						{
							Thing t = thingList[i];
							if (TradeUtility.EverTradeable(t.def) && t.def.category == ThingCategory.Item && !yieldedThings.Contains(t) && TradeUtility.TradeableNow(t))
							{
								yieldedThings.Add(t);
								yield return t;
							}
						}
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllSellableColonyPawns
		{
			get
			{
				foreach (Pawn p in Find.MapPawns.PrisonersOfColony)
				{
					if (p.Spawned && p.guest.PrisonerIsSecure)
					{
						yield return p;
					}
				}
				foreach (Pawn p2 in Find.MapPawns.PawnsInFaction(Faction.OfPlayer))
				{
					if (p2.Spawned && p2.RaceProps.Animal && p2.HostFaction == null && !p2.InMentalState && !p2.Downed)
					{
						yield return p2;
					}
				}
			}
		}

		public static bool TradeAsOne(Thing a, Thing b)
		{
			if (a.def.tradeNeverStack || b.def.tradeNeverStack)
			{
				return false;
			}
			if (a.def.category == ThingCategory.Pawn)
			{
				if (b.def != a.def)
				{
					return false;
				}
				if (a.def.race.Humanlike)
				{
					return false;
				}
				Pawn pawn = (Pawn)a;
				Pawn pawn2 = (Pawn)b;
				return pawn.kindDef == pawn2.kindDef && pawn.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn2.health.summaryHealth.SummaryHealthPercent >= 0.9999f && pawn.gender == pawn2.gender && (pawn.Name == null || pawn.Name.Numerical) && (pawn2.Name == null || pawn2.Name.Numerical) && pawn.ageTracker.CurLifeStageIndex == pawn2.ageTracker.CurLifeStageIndex && Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - pawn2.ageTracker.AgeBiologicalYearsFloat) <= 1f;
			}
			else
			{
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
					"Unknown TradeAsOne pair: ",
					a,
					", ",
					b
				}));
				return false;
			}
		}

		public static bool EverTradeable(ThingDef def)
		{
			return def.tradeability != Tradeability.Never && ((def.category == ThingCategory.Item || def.category == ThingCategory.Pawn) && def.GetStatValueAbstract(StatDefOf.MarketValue, null) > 0f);
		}

		public static void SpawnDropPod(IntVec3 dropSpot, Thing t)
		{
			DropPodUtility.MakeDropPodAt(dropSpot, new DropPodInfo
			{
				SingleContainedThing = t,
				leaveSlag = false
			});
		}

		public static float RandomPriceFactorFor(ITrader trader, Tradeable tradeable)
		{
			int num = DefDatabase<ThingDef>.AllDefsListForReading.IndexOf(tradeable.ThingDef);
			Rand.PushSeed();
			Rand.Seed = trader.RandomPriceFactorSeed * num;
			float result = Rand.Range(0.9f, 1.1f);
			Rand.PopSeed();
			return result;
		}

		public static Thing ThingFromStockMatching(ITrader trader, Thing thing)
		{
			if (thing is Pawn)
			{
				return null;
			}
			foreach (Thing current in trader.Goods)
			{
				if (TradeUtility.TradeAsOne(current, thing))
				{
					return current;
				}
			}
			return null;
		}

		public static bool TradeableNow(Thing t)
		{
			return !t.IsNotFresh();
		}

		public static void LaunchThingsOfType(ThingDef resDef, int debt, TradeShip trader)
		{
			while (debt > 0)
			{
				Thing thing = null;
				foreach (Building_OrbitalTradeBeacon current in Building_OrbitalTradeBeacon.AllPowered())
				{
					foreach (IntVec3 current2 in current.TradeableCells)
					{
						foreach (Thing current3 in Find.ThingGrid.ThingsAt(current2))
						{
							if (current3.def == resDef)
							{
								thing = current3;
								goto IL_C4;
							}
						}
					}
				}
				IL_C4:
				if (thing == null)
				{
					Log.Error("Could not find any " + resDef + " to transfer to trader.");
					break;
				}
				int num = Math.Min(debt, thing.stackCount);
				Thing thing2 = thing.SplitOff(num);
				if (trader != null)
				{
					trader.AddToStock(thing2);
				}
				debt -= num;
			}
		}

		public static void MakePrisonerOfColony(Pawn pawn)
		{
			if (pawn.Faction != null)
			{
				pawn.SetFaction(null, null);
			}
			pawn.guest.SetGuestStatus(Faction.OfPlayer, true);
			pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.Anesthetic, pawn, null), null, null);
		}

		public static void CheckInteractWithTradersTeachOpportunity(Pawn pawn)
		{
			if (pawn.Dead)
			{
				return;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.CurLordToil is LordToil_DefendTraderCaravan)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.InteractingWithTraders, pawn, OpportunityType.Important);
			}
		}
	}
}
