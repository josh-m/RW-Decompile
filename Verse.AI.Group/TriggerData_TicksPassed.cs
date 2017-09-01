using System;

namespace Verse.AI.Group
{
	public class TriggerData_TicksPassed : TriggerData
	{
		public int ticksPassed;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksPassed, "ticksPassed", 0, false);
		}
	}
}
