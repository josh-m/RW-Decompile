using System;
using UnityEngine;

namespace Verse.AI.Group
{
	public class Trigger_TicksPassed : Trigger
	{
		private int duration = 100;

		protected TriggerData_TicksPassed Data
		{
			get
			{
				return (TriggerData_TicksPassed)this.data;
			}
		}

		public int TicksLeft
		{
			get
			{
				return Mathf.Max(this.duration - this.Data.ticksPassed, 0);
			}
		}

		public Trigger_TicksPassed(int tickLimit)
		{
			this.data = new TriggerData_TicksPassed();
			this.duration = tickLimit;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick)
			{
				TriggerData_TicksPassed data = this.Data;
				data.ticksPassed++;
				return data.ticksPassed > this.duration;
			}
			return false;
		}

		public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
		{
			if (!transition.sources.Contains(previousToil))
			{
				this.Data.ticksPassed = 0;
			}
		}
	}
}
