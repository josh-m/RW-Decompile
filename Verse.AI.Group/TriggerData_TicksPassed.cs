using System;

namespace Verse.AI.Group
{
	public class TriggerData_TicksPassed : TriggerData
	{
		public int ticksPassed;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksPassed, "ticksPassed", 0, false);
		}
	}
}
