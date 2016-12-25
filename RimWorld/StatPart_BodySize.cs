using System;
using Verse;

namespace RimWorld
{
	public class StatPart_BodySize : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null)
				{
					val *= pawn.BodySize;
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.ageTracker != null)
				{
					return "StatsReport_BodySize".Translate(new object[]
					{
						pawn.BodySize.ToString("F2")
					}) + ": x" + pawn.BodySize.ToStringPercent();
				}
			}
			return null;
		}
	}
}
