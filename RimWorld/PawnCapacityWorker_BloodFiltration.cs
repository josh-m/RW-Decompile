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
			if (body.HasPartWithTag("BloodFiltrationKidney"))
			{
				return PawnCapacityUtility.CalculateTagEfficiency(diffSet, "BloodFiltrationKidney", 3.40282347E+38f, impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, "BloodFiltrationLiver", 3.40282347E+38f, impactors);
			}
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, "BloodFiltrationSource", 3.40282347E+38f, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return (body.HasPartWithTag("BloodFiltrationKidney") && body.HasPartWithTag("BloodFiltrationLiver")) || body.HasPartWithTag("BloodFiltrationSource");
		}
	}
}
