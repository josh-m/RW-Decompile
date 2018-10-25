using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_GoodwillImpact : CompTargetEffect
	{
		protected CompProperties_TargetEffect_GoodwillImpact PropsGoodwillImpact
		{
			get
			{
				return (CompProperties_TargetEffect_GoodwillImpact)this.props;
			}
		}

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.Faction != null && target.Faction != null && !target.Faction.HostileTo(user.Faction))
			{
				Faction arg_98_0 = target.Faction;
				Faction faction = user.Faction;
				int goodwillImpact = this.PropsGoodwillImpact.goodwillImpact;
				string reason = "GoodwillChangedReason_UsedItem".Translate(this.parent.LabelShort, target.LabelShort, this.parent.Named("ITEM"), target.Named("TARGET"));
				GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(target);
				arg_98_0.TryAffectGoodwillWith(faction, goodwillImpact, true, true, reason, lookTarget);
			}
		}
	}
}
