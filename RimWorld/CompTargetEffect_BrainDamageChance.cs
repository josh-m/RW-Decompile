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
				int num = Rand.RangeInclusive(1, 5);
				Thing arg_6B_0 = pawn;
				DamageDef flame = DamageDefOf.Flame;
				int amount = num;
				arg_6B_0.TakeDamage(new DamageInfo(flame, amount, -1f, user, brain, this.parent.def, DamageInfo.SourceCategory.ThingOrUnknown));
			}
		}
	}
}
