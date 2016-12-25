using System;
using Verse;

namespace RimWorld
{
	public class StatPart_NaturalNotMissingBodyPartsCoverage : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			float num;
			if (this.TryGetValue(req, out num))
			{
				val *= num;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			float f;
			if (this.TryGetValue(req, out f))
			{
				return "StatsReport_MissingBodyParts".Translate() + ": x" + f.ToStringPercent();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => x.health.hediffSet.GetCoverageOfNotMissingNaturalParts(x.RaceProps.body.corePart), (ThingDef x) => 1f, out value);
		}
	}
}
