using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Metabolism : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, "MetabolismSource", 3.40282347E+38f, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("MetabolismSource");
		}
	}
}
