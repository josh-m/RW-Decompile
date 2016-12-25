using System;
using Verse;

namespace RimWorld
{
	public class StatPart_UnpoweredWorkTable : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && this.ShouldApply(req.Thing))
			{
				val *= req.Thing.def.building.unpoweredWorkTableWorkSpeedFactor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && this.ShouldApply(req.Thing))
			{
				float unpoweredWorkTableWorkSpeedFactor = req.Thing.def.building.unpoweredWorkTableWorkSpeedFactor;
				return "StatsReport_UnpoweredWorkTable".Translate() + ": x" + unpoweredWorkTableWorkSpeedFactor.ToStringPercent();
			}
			return null;
		}

		private bool ShouldApply(Thing th)
		{
			if (th.def.building.unpoweredWorkTableWorkSpeedFactor == 0f)
			{
				return false;
			}
			CompPowerTrader compPowerTrader = th.TryGetComp<CompPowerTrader>();
			return compPowerTrader != null && !compPowerTrader.PowerOn;
		}
	}
}
