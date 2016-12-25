using System;
using Verse;

namespace RimWorld
{
	public class TradeableComparer_HitPointsPercentage : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			return this.GetValueFor(lhs).CompareTo(this.GetValueFor(rhs));
		}

		private float GetValueFor(Tradeable t)
		{
			Thing anyThing = t.AnyThing;
			Pawn pawn = anyThing as Pawn;
			if (pawn != null)
			{
				return pawn.health.summaryHealth.SummaryHealthPercent;
			}
			if (!anyThing.def.useHitPoints)
			{
				return 1f;
			}
			return (float)anyThing.HitPoints / (float)anyThing.MaxHitPoints;
		}
	}
}
