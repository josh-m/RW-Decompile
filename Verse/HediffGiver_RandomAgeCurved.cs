using System;

namespace Verse
{
	public class HediffGiver_RandomAgeCurved : HediffGiver
	{
		public SimpleCurve ageFractionMtbDaysCurve;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float x = (float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy;
			if (Rand.MTBEventOccurs(this.ageFractionMtbDaysCurve.Evaluate(x), 60000f, 60f) && base.TryApply(pawn, null))
			{
				base.SendLetter(pawn, cause);
			}
		}
	}
}
