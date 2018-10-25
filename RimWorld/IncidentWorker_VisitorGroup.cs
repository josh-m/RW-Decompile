using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_VisitorGroup : IncidentWorker_NeutralGroup
	{
		private const float TraderChance = 0.75f;

		private static readonly SimpleCurve PointsCurve = new SimpleCurve
		{
			{
				new CurvePoint(45f, 0f),
				true
			},
			{
				new CurvePoint(50f, 1f),
				true
			},
			{
				new CurvePoint(100f, 1f),
				true
			},
			{
				new CurvePoint(200f, 0.25f),
				true
			},
			{
				new CurvePoint(300f, 0.1f),
				true
			},
			{
				new CurvePoint(500f, 0f),
				true
			}
		};

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!base.TryResolveParms(parms))
			{
				return false;
			}
			List<Pawn> list = base.SpawnPawns(parms);
			if (list.Count == 0)
			{
				return false;
			}
			IntVec3 chillSpot;
			RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
			LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, chillSpot);
			LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
			bool flag = false;
			if (Rand.Value < 0.75f)
			{
				flag = this.TryConvertOnePawnToSmallTrader(list, parms.faction, map);
			}
			Pawn pawn = list.Find((Pawn x) => parms.faction.leader == x);
			string label;
			string text;
			if (list.Count == 1)
			{
				string value = (!flag) ? string.Empty : ("\n\n" + "SingleVisitorArrivesTraderInfo".Translate(list[0].Named("PAWN")).AdjustedFor(list[0], "PAWN"));
				string value2 = (pawn == null) ? string.Empty : ("\n\n" + "SingleVisitorArrivesLeaderInfo".Translate(list[0].Named("PAWN")).AdjustedFor(list[0], "PAWN"));
				label = "LetterLabelSingleVisitorArrives".Translate();
				text = "SingleVisitorArrives".Translate(list[0].story.Title, parms.faction.Name, list[0].Name.ToStringFull, value, value2, list[0].Named("PAWN")).AdjustedFor(list[0], "PAWN");
			}
			else
			{
				string value3 = (!flag) ? string.Empty : ("\n\n" + "GroupVisitorsArriveTraderInfo".Translate());
				string value4 = (pawn == null) ? string.Empty : ("\n\n" + "GroupVisitorsArriveLeaderInfo".Translate(pawn.LabelShort, pawn));
				label = "LetterLabelGroupVisitorsArrive".Translate();
				text = "GroupVisitorsArrive".Translate(parms.faction.Name, value3, value4);
			}
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref label, ref text, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, list[0], parms.faction, null);
			return true;
		}

		protected override void ResolveParmsPoints(IncidentParms parms)
		{
			if (parms.points >= 0f)
			{
				return;
			}
			parms.points = Rand.ByCurve(IncidentWorker_VisitorGroup.PointsCurve);
		}

		private bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction, Map map)
		{
			if (faction.def.visitorTraderKinds.NullOrEmpty<TraderKindDef>())
			{
				return false;
			}
			Pawn pawn = pawns.RandomElement<Pawn>();
			Lord lord = pawn.GetLord();
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
			TraderKindDef traderKindDef = faction.def.visitorTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality);
			pawn.trader.traderKind = traderKindDef;
			pawn.inventory.DestroyAll(DestroyMode.Vanish);
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = traderKindDef;
			parms.tile = new int?(map.Tile);
			parms.traderFaction = faction;
			foreach (Thing current in ThingSetMakerDefOf.TraderStock.root.Generate(parms))
			{
				Pawn pawn2 = current as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.Faction != pawn.Faction)
					{
						pawn2.SetFaction(pawn.Faction, null);
					}
					IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5, null);
					GenSpawn.Spawn(pawn2, loc, map, WipeMode.Vanish);
					lord.AddPawn(pawn2);
				}
				else if (!pawn.inventory.innerContainer.TryAdd(current, true))
				{
					current.Destroy(DestroyMode.Vanish);
				}
			}
			PawnInventoryGenerator.GiveRandomFood(pawn);
			return true;
		}
	}
}
