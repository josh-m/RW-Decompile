using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_SadisticRageTantrum : MentalStateWorker
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			MentalStateWorker_SadisticRageTantrum.tmpThings.Clear();
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, MentalStateWorker_SadisticRageTantrum.tmpThings, (Thing x) => TantrumMentalStateUtility.CanAttackPrisoner(pawn, x), 0, 40);
			bool result = MentalStateWorker_SadisticRageTantrum.tmpThings.Any<Thing>();
			MentalStateWorker_SadisticRageTantrum.tmpThings.Clear();
			return result;
		}
	}
}
