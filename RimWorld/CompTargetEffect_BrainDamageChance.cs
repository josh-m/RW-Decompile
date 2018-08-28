using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_BrainDamageChance : CompTargetEffect
	{
		protected CompProperties_TargetEffect_BrainDamageChance PropsBrainDamageChance
		{
			get
			{
				return (CompProperties_TargetEffect_BrainDamageChance)this.props;
			}
		}

		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Dead)
			{
				return;
			}
			if (Rand.Value <= this.PropsBrainDamageChance.brainDamageChance)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain == null)
				{
					return;
				}
				int num = Rand.RangeInclusive(1, 5);
				Thing arg_7C_0 = pawn;
				DamageDef flame = DamageDefOf.Flame;
				float amount = (float)num;
				BodyPartRecord hitPart = brain;
				arg_7C_0.TakeDamage(new DamageInfo(flame, amount, 0f, -1f, user, hitPart, this.parent.def, DamageInfo.SourceCategory.ThingOrUnknown, null));
			}
		}
	}
}
