using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Breathing : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyPartTagDef tag = BodyPartTagDefOf.BreathingSource;
			float arg_4E_0 = PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
			tag = BodyPartTagDefOf.BreathingPathway;
			float maximum = 1f;
			float arg_78_0 = arg_4E_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, maximum, default(FloatRange), impactors, -1f);
			tag = BodyPartTagDefOf.BreathingSourceCage;
			maximum = 1f;
			return arg_78_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, maximum, default(FloatRange), impactors, -1f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.BreathingSource);
		}
	}
}
