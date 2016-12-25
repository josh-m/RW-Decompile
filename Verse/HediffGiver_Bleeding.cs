using System;

namespace Verse
{
	public class HediffGiver_Bleeding : HediffGiver
	{
		public override bool CheckGiveEverySecond(Pawn pawn)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			bool flag = hediffSet.BleedingRate >= 0.1f;
			if (flag)
			{
				HealthUtility.AdjustSeverity(pawn, this.hediff, hediffSet.BleedingRate * 0.001f);
			}
			else
			{
				HealthUtility.AdjustSeverity(pawn, this.hediff, -0.02f);
			}
			return false;
		}
	}
}
