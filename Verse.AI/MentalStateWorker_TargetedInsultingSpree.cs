using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_TargetedInsultingSpree : MentalStateWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, MentalStateWorker_TargetedInsultingSpree.candidates, false);
			bool result = MentalStateWorker_TargetedInsultingSpree.candidates.Any<Pawn>();
			MentalStateWorker_TargetedInsultingSpree.candidates.Clear();
			return result;
		}
	}
}
