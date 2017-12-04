using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Breathing : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			string tag = "BreathingSource";
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, "BreathingPathway", 1f, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("BreathingSource");
		}
	}
}
