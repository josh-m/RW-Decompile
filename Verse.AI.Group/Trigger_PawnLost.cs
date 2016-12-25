using System;

namespace Verse.AI.Group
{
	public class Trigger_PawnLost : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.PawnLost;
		}
	}
}
