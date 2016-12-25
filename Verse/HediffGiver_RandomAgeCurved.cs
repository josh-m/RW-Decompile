using System;

namespace Verse
{
	public class HediffGiver_RandomAgeCurved : HediffGiver
	{
		public SimpleCurve ageFractionMtbDaysCurve;

		public override bool CheckGiveEverySecond(Pawn pawn)
		{
			float x = (float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy;
			return Rand.MTBEventOccurs(this.ageFractionMtbDaysCurve.Evaluate(x), 60000f, 60f) && base.TryApply(pawn, null);
		}
	}
}
