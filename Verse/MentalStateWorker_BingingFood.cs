using System;

namespace Verse
{
	public class MentalStateWorker_BingingFood : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && Find.ResourceCounter.TotalHumanEdibleNutrition > 10f;
		}
	}
}
