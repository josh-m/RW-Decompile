using System;
using Verse;

namespace RimWorld
{
	public class JobGiver_BingeFood : JobGiver_Binge
	{
		private const int BaseIngestInterval = 1100;

		protected override int IngestInterval(Pawn pawn)
		{
			return 1100;
		}

		protected override Thing BestIngestTarget(Pawn pawn)
		{
			Thing result;
			ThingDef thingDef;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out result, out thingDef, false, true, true, true))
			{
				return result;
			}
			return null;
		}
	}
}
