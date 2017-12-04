using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitPeaceTalks : CaravanArrivalAction
	{
		private PeaceTalks peaceTalks;

		public override string ReportString
		{
			get
			{
				return "CaravanVisiting".Translate(new object[]
				{
					this.peaceTalks.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.peaceTalks == null || !this.peaceTalks.Spawned;
			}
		}

		public CaravanArrivalAction_VisitPeaceTalks()
		{
		}

		public CaravanArrivalAction_VisitPeaceTalks(PeaceTalks peaceTalks)
		{
			this.peaceTalks = peaceTalks;
		}

		public override void Arrived(Caravan caravan)
		{
			this.peaceTalks.Notify_CaravanArrived(caravan);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<PeaceTalks>(ref this.peaceTalks, "peaceTalks", false);
		}
	}
}
