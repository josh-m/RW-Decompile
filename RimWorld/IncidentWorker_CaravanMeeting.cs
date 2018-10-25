using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_CaravanMeeting : IncidentWorker
	{
		private const int MapSize = 100;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Faction faction;
			return parms.target is Map || (CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile) && this.TryFindFaction(out faction));
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (parms.target is Map)
			{
				return IncidentDefOf.TravelerGroup.Worker.TryExecute(parms);
			}
			Caravan caravan = (Caravan)parms.target;
			Faction faction;
			if (!this.TryFindFaction(out faction))
			{
				return false;
			}
			CameraJumper.TryJumpAndSelect(caravan);
			List<Pawn> pawns = this.GenerateCaravanPawns(faction);
			Caravan metCaravan = CaravanMaker.MakeCaravan(pawns, faction, -1, false);
			string text = "CaravanMeeting".Translate(caravan.Name, faction.Name, PawnUtility.PawnKindsToCommaList(metCaravan.PawnsListForReading, true)).CapitalizeFirst();
			DiaNode diaNode = new DiaNode(text);
			Pawn bestPlayerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
			if (metCaravan.CanTradeNow)
			{
				DiaOption diaOption = new DiaOption("CaravanMeeting_Trade".Translate());
				diaOption.action = delegate
				{
					Find.WindowStack.Add(new Dialog_Trade(bestPlayerNegotiator, metCaravan, false));
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(metCaravan.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithOtherCaravan".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, false, true);
				};
				if (bestPlayerNegotiator == null)
				{
					diaOption.Disable("CaravanMeeting_TradeIncapable".Translate());
				}
				diaNode.options.Add(diaOption);
			}
			DiaOption diaOption2 = new DiaOption("CaravanMeeting_Attack".Translate());
			diaOption2.action = delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					Pawn t = caravan.PawnsListForReading[0];
					Faction arg_49_0 = faction;
					Faction ofPlayer = Faction.OfPlayer;
					FactionRelationKind kind = FactionRelationKind.Hostile;
					string reason = "GoodwillChangedReason_AttackedCaravan".Translate();
					arg_49_0.TrySetRelationKind(ofPlayer, kind, true, reason, new GlobalTargetInfo?(t));
					Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(100, 1, 100), WorldObjectDefOf.AttackedNonPlayerCaravan);
					IntVec3 playerSpot;
					IntVec3 enemySpot;
					MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerSpot, out enemySpot);
					CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(playerSpot, map, 12, null), CaravanDropInventoryMode.DoNotDrop, true);
					List<Pawn> list = metCaravan.PawnsListForReading.ToList<Pawn>();
					CaravanEnterMapUtility.Enter(metCaravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(enemySpot, map, 12, null), CaravanDropInventoryMode.DoNotDrop, false);
					LordMaker.MakeNewLord(faction, new LordJob_DefendAttackedTraderCaravan(list[0].Position), map, list);
					Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
					CameraJumper.TryJumpAndSelect(t);
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(list, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
				}, "GeneratingMapForNewEncounter", false, null);
			};
			diaOption2.resolveTree = true;
			diaNode.options.Add(diaOption2);
			DiaOption diaOption3 = new DiaOption("CaravanMeeting_MoveOn".Translate());
			diaOption3.action = delegate
			{
				this.RemoveAllPawnsAndPassToWorld(metCaravan);
			};
			diaOption3.resolveTree = true;
			diaNode.options.Add(diaOption3);
			string text2 = "CaravanMeetingTitle".Translate(caravan.Label);
			WindowStack arg_1F1_0 = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction faction2 = faction;
			bool delayInteractivity = true;
			string title = text2;
			arg_1F1_0.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, faction2, delayInteractivity, false, title));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, text2, faction));
			return true;
		}

		private bool TryFindFaction(out Faction faction)
		{
			return (from x in Find.FactionManager.AllFactionsListForReading
			where !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && !x.def.hidden && x.def.humanlikeFaction && x.def.caravanTraderKinds.Any<TraderKindDef>()
			select x).TryRandomElement(out faction);
		}

		private List<Pawn> GenerateCaravanPawns(Faction faction)
		{
			return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Trader,
				faction = faction,
				points = TraderCaravanUtility.GenerateGuardPoints(),
				dontUseSingleUseRocketLaunchers = true
			}, true).ToList<Pawn>();
		}

		private void RemoveAllPawnsAndPassToWorld(Caravan caravan)
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Find.WorldPawns.PassToWorld(pawnsListForReading[i], PawnDiscardDecideMode.Decide);
			}
			caravan.RemoveAllPawns();
		}
	}
}
