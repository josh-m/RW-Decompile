using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_GoodwillImpact : CompTargetEffect
	{
		public float goodwillImpact = -200f;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.Faction != null && target.Faction != null && user.Faction != target.Faction)
			{
				target.Faction.AffectGoodwillWith(user.Faction, this.goodwillImpact);
			}
		}
	}
}
