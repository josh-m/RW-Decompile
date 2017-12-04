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
			string tag;
			if (body.HasPartWithTag("BloodFiltrationKidney"))
			{
				tag = "BloodFiltrationKidney";
				float arg_4F_0 = PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors);
				tag = "BloodFiltrationLiver";
				return arg_4F_0 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors);
			}
			tag = "BloodFiltrationSource";
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, tag, 3.40282347E+38f, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return (body.HasPartWithTag("BloodFiltrationKidney") && body.HasPartWithTag("BloodFiltrationLiver")) || body.HasPartWithTag("BloodFiltrationSource");
		}
	}
}
