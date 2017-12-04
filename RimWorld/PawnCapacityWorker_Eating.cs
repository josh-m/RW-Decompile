using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Eating : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			string tag = "EatingSource";
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, "EatingPathway", 1f, impactors) * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("EatingSource");
		}
	}
}
