using System;
using Verse;

namespace RimWorld
{
	public class StatPart_NaturalNotMissingBodyPartsCoverage : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null)
				{
					val *= this.ValueFor(pawn);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null)
				{
					return "StatsReport_MissingBodyParts".Translate() + ": x" + this.ValueFor(pawn).ToStringPercent();
				}
			}
			return null;
		}

		private float ValueFor(Pawn pawn)
		{
			return pawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart);
		}
	}
}
