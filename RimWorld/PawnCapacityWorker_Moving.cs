using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Moving : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			float num = 0f;
			float num2 = PawnCapacityUtility.CalculateLimbEfficiency(diffSet, BodyPartTagDefOf.MovingLimbCore, BodyPartTagDefOf.MovingLimbSegment, BodyPartTagDefOf.MovingLimbDigit, 0.4f, out num, impactors);
			if (num < 0.4999f)
			{
				return 0f;
			}
			float arg_5E_0 = num2;
			BodyPartTagDef tag = BodyPartTagDefOf.Pelvis;
			num2 = arg_5E_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
			float arg_89_0 = num2;
			tag = BodyPartTagDefOf.Spine;
			num2 = arg_89_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
			num2 = Mathf.Lerp(num2, num2 * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Breathing, impactors), 0.2f);
			num2 = Mathf.Lerp(num2, num2 * base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.BloodPumping, impactors), 0.2f);
			return num2 * Mathf.Min(base.CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors), 1f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.MovingLimbCore);
		}
	}
}
