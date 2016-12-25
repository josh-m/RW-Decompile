using System;
using Verse;

namespace RimWorld
{
	public class StatPart_Health : StatPart_Curve
	{
		protected override bool AppliesTo(StatRequest req)
		{
			return req.HasThing && req.Thing.def.useHitPoints;
		}

		protected override float CurveXGetter(StatRequest req)
		{
			return (float)req.Thing.HitPoints / (float)req.Thing.MaxHitPoints;
		}

		protected override string ExplanationLabel(StatRequest req)
		{
			return "StatsReport_HealthMultiplier".Translate(new object[]
			{
				req.Thing.HitPoints + " / " + req.Thing.MaxHitPoints
			});
		}
	}
}
