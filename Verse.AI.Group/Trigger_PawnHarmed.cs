using System;

namespace Verse.AI.Group
{
	public class Trigger_PawnHarmed : Trigger
	{
		public float chance = 1f;

		public bool requireInstigatorWithFaction;

		public Trigger_PawnHarmed(float chance = 1f, bool requireInstigatorWithFaction = false)
		{
			this.chance = chance;
			this.requireInstigatorWithFaction = requireInstigatorWithFaction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return Trigger_PawnHarmed.SignalIsHarm(signal) && (!this.requireInstigatorWithFaction || (signal.dinfo.Instigator != null && signal.dinfo.Instigator.Faction != null)) && Rand.Value < this.chance;
		}

		public static bool SignalIsHarm(TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnDamaged)
			{
				return signal.dinfo.Def.externalViolence;
			}
			if (signal.type == TriggerSignalType.PawnLost)
			{
				return signal.condition == PawnLostCondition.MadePrisoner || signal.condition == PawnLostCondition.IncappedOrKilled;
			}
			return signal.type == TriggerSignalType.PawnArrestAttempted;
		}
	}
}
