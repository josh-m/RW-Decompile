using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_BloodPumping : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			string tag = "BloodPumpingSource";
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("BloodPumpingSource");
		}
	}
}
