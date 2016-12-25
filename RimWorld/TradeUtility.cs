using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class TradeUtility
	{
		public static bool EverTradeable(ThingDef def)
		{
			return def.tradeability != Tradeability.Never && ((def.category == ThingCategory.Item || def.category == ThingCategory.Pawn) && def.GetStatValueAbstract(StatDefOf.MarketValue, null) > 0f);
		}

		public static void SpawnDropPod(IntVec3 dropSpot, Map map, Thing t)
		{
			DropPodUtility.MakeDropPodAt(dropSpot, map, new ActiveDropPodInfo
			{
				SingleContainedThing = t,
				leaveSlag = false
			});
		}

		[DebuggerHidden]
		public static IEnumerable<Thing> AllLaunchableThings(Map map)
		{
			HashSet<Thing> yieldedThings = new HashSet<Thing>();
			foreach (Building_OrbitalTradeBeacon beacon in Building_OrbitalTradeBeacon.AllPowered(map))
			{
				foreach (IntVec3 c in beacon.TradeableCells)
				{
					List<Thing> thingList = c.GetThingList(map);
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

		[DebuggerHidden]
		public static IEnumerable<Pawn> AllSellableColonyPawns(Map map)
		{
			foreach (Pawn p in map.mapPawns.PrisonersOfColonySpawned)
			{
				if (p.guest.PrisonerIsSecure)
				{
					yield return p;
				}
			}
			foreach (Pawn p2 in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (p2.RaceProps.Animal && p2.HostFaction == null && !p2.InMentalState && !p2.Downed)
				{
					yield return p2;
				}
			}
		}

		public static Thing ThingFromStockMatching(ITrader trader, Thing thing)
		{
			if (thing is Pawn)
			{
				return null;
			}
			foreach (Thing current in trader.Goods)
			{
				if (TransferableUtility.TransferAsOne(current, thing))
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

		public static void LaunchThingsOfType(ThingDef resDef, int debt, Map map, TradeShip trader)
		{
			while (debt > 0)
			{
				Thing thing = null;
				foreach (Building_OrbitalTradeBeacon current in Building_OrbitalTradeBeacon.AllPowered(map))
				{
					foreach (IntVec3 current2 in current.TradeableCells)
					{
						foreach (Thing current3 in map.thingGrid.ThingsAt(current2))
						{
							if (current3.def == resDef)
							{
								thing = current3;
								goto IL_C6;
							}
						}
					}
				}
				IL_C6:
				if (thing == null)
				{
					Log.Error("Could not find any " + resDef + " to transfer to trader.");
					break;
				}
				int num = Math.Min(debt, thing.stackCount);
				Thing thing2 = thing.SplitOff(num);
				if (trader != null)
				{
					trader.AddToStock(thing2, TradeSession.playerNegotiator);
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
