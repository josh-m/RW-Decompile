using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitSettlement : CaravanArrivalAction
	{
		private SettlementBase settlement;

		public override string Label
		{
			get
			{
				return "VisitSettlement".Translate(this.settlement.Label);
			}
		}

		public override string ReportString
		{
			get
			{
				return "CaravanVisiting".Translate(this.settlement.Label);
			}
		}

		public CaravanArrivalAction_VisitSettlement()
		{
		}

		public CaravanArrivalAction_VisitSettlement(SettlementBase settlement)
		{
			this.settlement = settlement;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.settlement != null && this.settlement.Tile != destinationTile)
			{
				return false;
			}
			return CaravanArrivalAction_VisitSettlement.CanVisit(caravan, this.settlement);
		}

		public override void Arrived(Caravan caravan)
		{
			if (caravan.IsPlayerControlled)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(this.settlement), "LetterCaravanEnteredMap".Translate(caravan.Label, this.settlement).CapitalizeFirst(), LetterDefOf.NeutralEvent, caravan, null, null);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<SettlementBase>(ref this.settlement, "settlement", false);
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, SettlementBase settlement)
		{
			return settlement != null && settlement.Spawned && settlement.Visitable;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, SettlementBase settlement)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_VisitSettlement>(() => CaravanArrivalAction_VisitSettlement.CanVisit(caravan, settlement), () => new CaravanArrivalAction_VisitSettlement(settlement), "VisitSettlement".Translate(settlement.Label), caravan, settlement.Tile, settlement);
		}
	}
}
