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

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return CaravanRandomEncounterUtility.CanMeetRandomCaravanAt(target.Tile);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Caravan caravan = (Caravan)parms.target;
			Faction faction;
			if (!(from x in Find.FactionManager.AllFactionsListForReading
			where !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && !x.def.hidden && x.def.humanlikeFaction && x.def.caravanTraderKinds.Any<TraderKindDef>()
			select x).TryRandomElement(out faction))
			{
				return false;
			}
			JumpToTargetUtility.TryJumpAndSelect(caravan);
			List<Pawn> pawns = this.GenerateCaravanPawns(faction);
			Caravan metCaravan = CaravanMaker.MakeCaravan(pawns, faction, -1, false);
			string text = "CaravanMeeting".Translate(new object[]
			{
				faction.Name,
				PawnUtility.PawnKindsToCommaList(metCaravan.PawnsListForReading)
			});
			DiaNode diaNode = new DiaNode(text);
			Pawn bestPlayerNegotiator = CaravanVisitUtility.BestNegotiator(caravan);
			if (bestPlayerNegotiator != null && metCaravan.CanTradeNow)
			{
				DiaOption diaOption = new DiaOption("CaravanMeeting_Trade".Translate());
				diaOption.action = delegate
				{
					Find.WindowStack.Add(new Dialog_Trade(bestPlayerNegotiator, metCaravan));
					string empty = string.Empty;
					string empty2 = string.Empty;
					PawnRelationUtility.Notify_PawnsSeenByPlayer(metCaravan.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradingWithOtherCaravan".Translate(), false);
					if (!empty2.NullOrEmpty())
					{
						Find.LetterStack.ReceiveLetter(empty, empty2, LetterType.Good, new GlobalTargetInfo(caravan.Tile), null);
					}
				};
				diaNode.options.Add(diaOption);
			}
			DiaOption diaOption2 = new DiaOption("CaravanMeeting_Attack".Translate());
			diaOption2.action = delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					if (!faction.HostileTo(Faction.OfPlayer))
					{
						faction.SetHostileTo(Faction.OfPlayer, true);
					}
					Pawn t = caravan.PawnsListForReading[0];
					Map map = CaravanTargetIncidentUtility.GenerateOrGetMapForIncident(100, 100, caravan, CaravanEnterMode.None, WorldObjectDefOf.AttackedCaravan, null, false);
					IntVec3 playerSpot;
					IntVec3 enemySpot;
					MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerSpot, out enemySpot);
					CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(playerSpot, map, 12), CaravanDropInventoryMode.DoNotDrop, true);
					List<Pawn> list = metCaravan.PawnsListForReading.ToList<Pawn>();
					CaravanEnterMapUtility.Enter(metCaravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(enemySpot, map, 12), CaravanDropInventoryMode.DoNotDrop, false);
					LordMaker.MakeNewLord(faction, new LordJob_DefendAttackedTraderCaravan(list[0].Position), map, list);
					Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
					JumpToTargetUtility.TryJumpAndSelect(t);
					Messages.Message("MessageAttackedCaravanIsNowHostile".Translate(new object[]
					{
						faction.Name
					}), new GlobalTargetInfo(list[0].Position, list[0].Map, false), MessageSound.SeriousAlert);
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
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false));
			return true;
		}

		private List<Pawn> GenerateCaravanPawns(Faction faction)
		{
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.faction = faction;
			return PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Trader, pawnGroupMakerParms, true).ToList<Pawn>();
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
