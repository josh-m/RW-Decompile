using System;

namespace Verse
{
	public class HediffGiver_Random : HediffGiver
	{
		public float mtbDays;

		public override bool CheckGiveEverySecond(Pawn pawn)
		{
			return Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn, null);
		}
	}
}
