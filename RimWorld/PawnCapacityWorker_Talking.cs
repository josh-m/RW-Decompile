using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Talking : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, "TalkingSource", 3.40282347E+38f, impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, "TalkingPathway", 1f, impactors) * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("TalkingSource");
		}
	}
}
