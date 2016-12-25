using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_BrainDamageChance : CompTargetEffect
	{
		private const float Chance = 0.3f;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Dead)
			{
				return;
			}
			if (Rand.Value <= 0.3f)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain == null)
				{
					return;
				}
				BodyPartDamageInfo value = new BodyPartDamageInfo(brain, false, null);
				int amount = Rand.RangeInclusive(1, 5);
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, amount, user, new BodyPartDamageInfo?(value), this.parent.def);
				pawn.TakeDamage(dinfo);
			}
		}
	}
}
