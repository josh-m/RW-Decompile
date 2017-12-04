using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StatPart_Mood : StatPart
	{
		private SimpleCurve factorFromMoodCurve;

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.factorFromMoodCurve == null)
			{
				yield return "curve is null.";
			}
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && this.ActiveFor(pawn))
				{
					val *= this.FactorFromMood(pawn);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && this.ActiveFor(pawn))
				{
					return "StatsReport_MoodMultiplier".Translate(new object[]
					{
						pawn.needs.mood.CurLevel.ToStringPercent()
					}) + ": x" + this.FactorFromMood(pawn).ToStringPercent();
				}
			}
			return null;
		}

		private bool ActiveFor(Pawn pawn)
		{
			return pawn.needs.mood != null;
		}

		private float FactorFromMood(Pawn pawn)
		{
			return this.factorFromMoodCurve.Evaluate(pawn.needs.mood.CurLevel);
		}
	}
}
