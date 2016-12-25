using System;

namespace Verse.AI.Group
{
	public class Trigger_ChanceOnSignal : Trigger
	{
		private TriggerSignalType signalType;

		private float chance;

		public Trigger_ChanceOnSignal(TriggerSignalType signalType, float chance)
		{
			this.signalType = signalType;
			this.chance = chance;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == this.signalType && Rand.Value < this.chance;
		}
	}
}
