using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitSite : CaravanArrivalAction
	{
		private Site site;

		public override string ReportString
		{
			get
			{
				return (!this.site.KnownDanger) ? "CaravanVisiting".Translate(new object[]
				{
					this.site.Label
				}) : "CaravanAttacking".Translate(new object[]
				{
					this.site.Label
				});
			}
		}

		public override bool ShouldFail
		{
			get
			{
				return base.ShouldFail || this.site == null || !this.site.Spawned;
			}
		}

		public CaravanArrivalAction_VisitSite()
		{
		}

		public CaravanArrivalAction_VisitSite(Site site)
		{
			this.site = site;
		}

		public override void Arrived(Caravan caravan)
		{
			this.site.core.Worker.VisitAction(caravan, this.site);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Site>(ref this.site, "site", false);
		}
	}
}
