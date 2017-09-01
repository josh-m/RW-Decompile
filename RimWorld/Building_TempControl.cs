using System;
using Verse;

namespace RimWorld
{
	public class Building_TempControl : Building
	{
		public CompTempControl compTempControl;

		public CompPowerTrader compPowerTrader;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.compTempControl = base.GetComp<CompTempControl>();
			this.compPowerTrader = base.GetComp<CompPowerTrader>();
		}
	}
}
