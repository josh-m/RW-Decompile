using System;

namespace Verse.AI.Group
{
	public abstract class Trigger
	{
		public TriggerData data;

		public abstract bool ActivateOn(Lord lord, TriggerSignal signal);

		public virtual void SourceToilBecameActive(Transition transition, LordToil previousToil)
		{
		}

		public override string ToString()
		{
			return base.GetType().ToString();
		}
	}
}
