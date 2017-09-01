using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Manipulation : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			float num = 0f;
			float num2 = PawnCapacityUtility.CalculateLimbEfficiency(diffSet, "ManipulationLimbCore", "ManipulationLimbSegment", "ManipulationLimbDigit", 0.8f, out num, impactors);
			return num2 * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("ManipulationLimbCore");
		}
	}
}
