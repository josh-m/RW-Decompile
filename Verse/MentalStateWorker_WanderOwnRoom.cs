using System;

namespace Verse
{
	public class MentalStateWorker_WanderOwnRoom : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && pawn.ownership.OwnedBed != null;
		}
	}
}
