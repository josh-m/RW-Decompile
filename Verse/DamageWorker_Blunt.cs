using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DamageWorker_Blunt : DamageWorker_AddInjury
	{
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			bool flag = Rand.Chance(this.def.bluntInnerHitFrequency);
			float num = (!flag) ? 0f : this.def.bluntInnerHitConverted.RandomInRange;
			float num2 = totalDamage * (1f - num);
			while (true)
			{
				num2 -= base.FinalizeAndAddInjury(pawn, num2, dinfo, ref result);
				if (!pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
				{
					break;
				}
				if (num2 <= 1f)
				{
					break;
				}
				BodyPartRecord parent = dinfo.HitPart.parent;
				if (parent == null)
				{
					break;
				}
				dinfo.SetHitPart(parent);
			}
			if (flag && !dinfo.HitPart.def.IsSolid(dinfo.HitPart, pawn.health.hediffSet.hediffs) && dinfo.HitPart.depth == BodyPartDepth.Outside)
			{
				IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where x.parent == dinfo.HitPart && x.def.IsSolid(x, pawn.health.hediffSet.hediffs) && x.depth == BodyPartDepth.Inside
				select x;
				BodyPartRecord hitPart;
				if (source.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out hitPart))
				{
					DamageInfo lastInfo = dinfo;
					lastInfo.SetHitPart(hitPart);
					base.FinalizeAndAddInjury(pawn, totalDamage * num + totalDamage * this.def.bluntInnerHitAdded.RandomInRange, lastInfo, ref result);
				}
			}
		}
	}
}
