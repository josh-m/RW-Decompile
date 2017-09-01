using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StatPart_Age : StatPart
	{
		private SimpleCurve curve;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.ageTracker != null)
				{
					val *= this.AgeMultiplier(pawn);
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
					}) + ": x" + this.AgeMultiplier(pawn).ToStringPercent();
				}
			}
			return null;
		}

		private float AgeMultiplier(Pawn pawn)
		{
			return this.curve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.curve == null)
			{
				yield return "curve is null.";
			}
		}
	}
}
