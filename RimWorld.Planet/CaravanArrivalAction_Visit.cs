using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_Visit : CaravanArrivalAction
	{
		private FactionBase factionBase;

		public override string ReportString
		{
			get
			{
				return "CaravanVisiting".Translate(new object[]
				{
					this.factionBase.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.factionBase == null || !this.factionBase.Spawned;
			}
		}

		public CaravanArrivalAction_Visit()
		{
		}

		public CaravanArrivalAction_Visit(FactionBase factionBase)
		{
			this.factionBase = factionBase;
		}

		public override void Arrived(Caravan caravan)
		{
			if (caravan.IsPlayerControlled)
			{
				Messages.Message("MessageCaravanIsVisitingFaction".Translate(new object[]
				{
					caravan.Label,
					this.factionBase.Label
				}).CapitalizeFirst(), caravan, MessageSound.Benefit);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<FactionBase>(ref this.factionBase, "factionBase", false);
		}
	}
}
