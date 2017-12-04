using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_TargetedTantrum : MentalStateWorker
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			MentalStateWorker_TargetedTantrum.tmpThings.Clear();
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, MentalStateWorker_TargetedTantrum.tmpThings, null, 300, 40);
			bool result = MentalStateWorker_TargetedTantrum.tmpThings.Any<Thing>();
			MentalStateWorker_TargetedTantrum.tmpThings.Clear();
			return result;
		}
	}
}
