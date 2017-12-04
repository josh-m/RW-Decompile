using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_InsultingSpreeAll : MentalStateWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, MentalStateWorker_InsultingSpreeAll.candidates, true);
			bool result = MentalStateWorker_InsultingSpreeAll.candidates.Count >= 2;
			MentalStateWorker_InsultingSpreeAll.candidates.Clear();
			return result;
		}
	}
}
