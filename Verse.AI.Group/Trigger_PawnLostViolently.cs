using System;

namespace Verse.AI.Group
{
	public class Trigger_PawnLostViolently : Trigger
	{
		public bool allowRoofCollapse;

		public Trigger_PawnLostViolently(bool allowRoofCollapse = true)
		{
			this.allowRoofCollapse = allowRoofCollapse;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				if (signal.condition == PawnLostCondition.MadePrisoner)
				{
					return true;
				}
				if (signal.condition == PawnLostCondition.IncappedOrKilled)
				{
					bool flag = signal.dinfo.Category == DamageInfo.SourceCategory.Collapse;
					if (!flag || this.allowRoofCollapse)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
