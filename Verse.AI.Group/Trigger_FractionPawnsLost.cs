using System;

namespace Verse.AI.Group
{
	public class Trigger_FractionPawnsLost : Trigger
	{
		private float fraction = 0.5f;

		public Trigger_FractionPawnsLost(float fraction)
		{
			this.fraction = fraction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.PawnLost && (float)lord.numPawnsLostViolently >= (float)lord.numPawnsEverGained * this.fraction;
		}
	}
}
