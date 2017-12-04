using System;

namespace Verse
{
	public class DamageWorker_Bite : DamageWorker_AddInjury
	{
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			totalDamage *= this.def.biteDamageMultiplier;
			base.FinalizeAndAddInjury(pawn, totalDamage, dinfo, ref result);
		}
	}
}
