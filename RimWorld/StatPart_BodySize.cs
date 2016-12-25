using System;
using Verse;

namespace RimWorld
{
	public class StatPart_BodySize : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			float num;
			if (this.TryGetBodySize(req, out num))
			{
				val *= num;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			float f;
			if (this.TryGetBodySize(req, out f))
			{
				return "StatsReport_BodySize".Translate(new object[]
				{
					f.ToString("F2")
				}) + ": x" + f.ToStringPercent();
			}
			return null;
		}

		private bool TryGetBodySize(StatRequest req, out float bodySize)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => x.BodySize, (ThingDef x) => x.race.baseBodySize, out bodySize);
		}
	}
}
