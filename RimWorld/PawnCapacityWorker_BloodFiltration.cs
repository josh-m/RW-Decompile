using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_BloodFiltration : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			BodyPartTagDef tag;
			if (body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney))
			{
				tag = BodyPartTagDefOf.BloodFiltrationKidney;
				float arg_6D_0 = PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
				tag = BodyPartTagDefOf.BloodFiltrationLiver;
				return arg_6D_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
			}
			tag = BodyPartTagDefOf.BloodFiltrationSource;
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, default(FloatRange), impactors, -1f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return (body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney) && body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationLiver)) || body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationSource);
		}
	}
}
