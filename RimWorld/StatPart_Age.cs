using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StatPart_Age : StatPart
	{
		private SimpleCurve curve;

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.curve == null)
			{
				yield return "curve is null.";
			}
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.ageTracker != null)
				{
					val *= this.AgeMultiplier(pawn.ageTracker.AgeBiologicalYears);
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
					return "StatsReport_AgeMultiplier".Translate(new object[]
					{
						pawn.ageTracker.AgeBiologicalYears
					}) + ": x" + this.AgeMultiplier(pawn.ageTracker.AgeBiologicalYears).ToStringPercent();
				}
			}
			return null;
		}

		private float AgeMultiplier(int age)
		{
			return this.curve.Evaluate((float)age);
		}
	}
}
