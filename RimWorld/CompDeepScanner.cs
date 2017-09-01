using System;
using Verse;

namespace RimWorld
{
	public class CompDeepScanner : ThingComp
	{
		private CompPowerTrader powerComp;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.powerComp = this.parent.GetComp<CompPowerTrader>();
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if (this.powerComp.PowerOn)
			{
				this.parent.Map.deepResourceGrid.MarkForDraw();
			}
		}
	}
}
