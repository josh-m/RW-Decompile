using System;

namespace Verse.AI.Group
{
	public class Trigger_ThingDamageTaken : Trigger
	{
		private Thing thing;

		private float damageFraction = 0.5f;

		public Trigger_ThingDamageTaken(Thing thing, float damageFraction)
		{
			this.thing = thing;
			this.damageFraction = damageFraction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.Tick && (this.thing.DestroyedOrNull() || (float)this.thing.HitPoints < (1f - this.damageFraction) * (float)this.thing.MaxHitPoints);
		}
	}
}
