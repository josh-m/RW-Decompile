using System;

namespace Verse
{
	public class HediffGiver_Random : HediffGiver
	{
		public float mtbDays;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn, null))
			{
				base.SendLetter(pawn, cause);
			}
		}
	}
}
