using RimWorld;
using System;

namespace Verse
{
	public class CompHeatPusherPowered : CompHeatPusher
	{
		protected CompPowerTrader powerComp;

		protected CompRefuelable refuelableComp;

		protected CompBreakdownable breakdownableComp;

		protected CompFlickable flickableComp;

		protected override bool ShouldPushHeatNow
		{
			get
			{
				return (this.powerComp == null || this.powerComp.PowerOn) && (this.flickableComp == null || this.flickableComp.SwitchIsOn) && (this.refuelableComp == null || this.refuelableComp.HasFuel) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
			}
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			this.powerComp = this.parent.GetComp<CompPowerTrader>();
			this.refuelableComp = this.parent.GetComp<CompRefuelable>();
			this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
			this.flickableComp = this.parent.GetComp<CompFlickable>();
		}
	}
}
