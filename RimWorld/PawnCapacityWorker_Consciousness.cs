using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Consciousness : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, "ConsciousnessSource", 3.40282347E+38f, impactors);
			float num2 = Mathf.Clamp(GenMath.LerpDouble(0.1f, 1f, 0f, 0.4f, diffSet.PainTotal), 0f, 0.4f);
			if ((double)num2 >= 0.01)
			{
				num -= num2;
				if (impactors != null)
				{
					impactors.Add(new PawnCapacityUtility.CapacityImpactorPain());
				}
			}
			num = Mathf.Lerp(num, num * Mathf.Min(base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.BloodPumping, impactors), 1f), 0.2f);
			num = Mathf.Lerp(num, num * Mathf.Min(base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Breathing, impactors), 1f), 0.2f);
			return Mathf.Lerp(num, num * Mathf.Min(base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.BloodFiltration, impactors), 1f), 0.1f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag("ConsciousnessSource");
		}
	}
}
