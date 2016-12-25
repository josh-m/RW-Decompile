using System;

namespace RimWorld
{
	public class CompPowerPlant : CompPowerTrader
	{
		protected CompRefuelable refuelableComp;

		protected CompBreakdownable breakdownableComp;

		protected virtual float DesiredPowerOutput
		{
			get
			{
				return -base.Props.basePowerConsumption;
			}
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			this.refuelableComp = this.parent.GetComp<CompRefuelable>();
			this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
			if (base.Props.basePowerConsumption < 0f && !this.parent.IsBrokenDown())
			{
				base.PowerOn = true;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.refuelableComp != null && !this.refuelableComp.HasFuel) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
			{
				base.PowerOutput = 0f;
			}
			else
			{
				base.PowerOutput = this.DesiredPowerOutput;
			}
		}
	}
}
