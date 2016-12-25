using System;

namespace Verse
{
	public class MentalStateWorker_BingingFood : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && (!pawn.Spawned || pawn.Map.resourceCounter.TotalHumanEdibleNutrition > 10f);
		}
	}
}
