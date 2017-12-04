using System;

namespace Verse.AI.Group
{
	public class Trigger_TickCondition : Trigger
	{
		private Func<bool> condition;

		private int checkEveryTicks = 1;

		public Trigger_TickCondition(Func<bool> condition, int checkEveryTicks = 1)
		{
			this.condition = condition;
			this.checkEveryTicks = checkEveryTicks;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % this.checkEveryTicks == 0 && this.condition();
		}
	}
}
