using System;

namespace Verse.AI.Group
{
	public class Trigger_Memo : Trigger
	{
		private string memo;

		public Trigger_Memo(string memo)
		{
			this.memo = memo;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.Memo && signal.memo == this.memo;
		}
	}
}
