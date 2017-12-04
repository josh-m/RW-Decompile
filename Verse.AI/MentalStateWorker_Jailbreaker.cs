using System;

namespace Verse.AI
{
	public class MentalStateWorker_Jailbreaker : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && JailbreakerMentalStateUtility.FindPrisoner(pawn) != null;
		}
	}
}
