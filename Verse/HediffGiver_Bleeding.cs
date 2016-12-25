using System;

namespace Verse
{
	public class HediffGiver_Bleeding : HediffGiver
	{
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			bool flag = hediffSet.BleedRateTotal >= 0.1f;
			if (flag)
			{
				HealthUtility.AdjustSeverity(pawn, this.hediff, hediffSet.BleedRateTotal * 0.001f);
			}
			else
			{
				HealthUtility.AdjustSeverity(pawn, this.hediff, -0.02f);
			}
		}
	}
}
