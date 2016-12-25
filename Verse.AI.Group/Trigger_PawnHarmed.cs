using System;

namespace Verse.AI.Group
{
	public class Trigger_PawnHarmed : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return Trigger_PawnHarmed.SignalIsHarm(signal);
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
