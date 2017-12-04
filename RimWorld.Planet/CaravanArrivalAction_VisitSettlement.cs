using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitSettlement : CaravanArrivalAction
	{
		private Settlement settlement;

		public override string ReportString
		{
			get
			{
				return "CaravanVisiting".Translate(new object[]
				{
					this.settlement.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.settlement == null || !this.settlement.Spawned || !this.settlement.Visitable;
			}
		}

		public CaravanArrivalAction_VisitSettlement()
		{
		}

		public CaravanArrivalAction_VisitSettlement(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public override void Arrived(Caravan caravan)
		{
			if (caravan.IsPlayerControlled)
			{
				Messages.Message("MessageCaravanIsVisitingSettlement".Translate(new object[]
				{
					caravan.Label,
					this.settlement.Label
				}).CapitalizeFirst(), caravan, MessageTypeDefOf.TaskCompletion);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
		}
	}
}
