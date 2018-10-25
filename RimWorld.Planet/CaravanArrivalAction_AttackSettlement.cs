using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_AttackSettlement : CaravanArrivalAction
	{
		private SettlementBase settlement;

		public override string Label
		{
			get
			{
				return "AttackSettlement".Translate(this.settlement.Label);
			}
		}

		public override string ReportString
		{
			get
			{
				return "CaravanAttacking".Translate(this.settlement.Label);
			}
		}

		public CaravanArrivalAction_AttackSettlement()
		{
		}

		public CaravanArrivalAction_AttackSettlement(SettlementBase settlement)
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
			return CaravanArrivalAction_AttackSettlement.CanAttack(caravan, this.settlement);
		}

		public override void Arrived(Caravan caravan)
		{
			SettlementUtility.Attack(caravan, this.settlement);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<SettlementBase>(ref this.settlement, "settlement", false);
		}

		public static FloatMenuAcceptanceReport CanAttack(Caravan caravan, SettlementBase settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Attackable)
			{
				return false;
			}
			if (settlement.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownDaysLeft().ToString("0.#")));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, SettlementBase settlement)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_AttackSettlement>(() => CaravanArrivalAction_AttackSettlement.CanAttack(caravan, settlement), () => new CaravanArrivalAction_AttackSettlement(settlement), "AttackSettlement".Translate(settlement.Label), caravan, settlement.Tile, settlement);
		}
	}
}
