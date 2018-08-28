using System;
using Verse;

namespace RimWorld
{
	public class StatPart_WildManOffset : StatPart
	{
		public float offset;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (this.IsWildMan(req))
			{
				val += this.offset;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (this.IsWildMan(req))
			{
				return "StatsReport_WildMan".Translate() + ": " + this.offset.ToStringWithSign("0.##");
			}
			return null;
		}

		private bool IsWildMan(StatRequest req)
		{
			Pawn pawn = req.Thing as Pawn;
			return pawn != null && pawn.IsWildMan();
		}
	}
}
