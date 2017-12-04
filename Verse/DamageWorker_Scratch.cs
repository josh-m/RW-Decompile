using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DamageWorker_Scratch : DamageWorker_AddInjury
	{
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			if (dinfo.HitPart.depth == BodyPartDepth.Inside)
			{
				List<BodyPartRecord> list = new List<BodyPartRecord>();
				for (BodyPartRecord bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
				{
					list.Add(bodyPartRecord);
					if (bodyPartRecord.depth == BodyPartDepth.Outside)
					{
						break;
					}
				}
				float num = (float)list.Count;
				for (int i = 0; i < list.Count; i++)
				{
					DamageInfo dinfo2 = dinfo;
					dinfo2.SetHitPart(list[i]);
					base.FinalizeAndAddInjury(pawn, totalDamage / num, dinfo2, ref result);
				}
			}
			else
			{
				IEnumerable<BodyPartRecord> enumerable = dinfo.HitPart.GetDirectChildParts();
				if (dinfo.HitPart.parent != null)
				{
					enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
					enumerable = enumerable.Concat(dinfo.HitPart.parent);
				}
				enumerable = from target in enumerable
				where target != dinfo.HitPart && !target.def.isConceptual && target.depth == BodyPartDepth.Outside && !pawn.health.hediffSet.PartIsMissing(target)
				select target;
				BodyPartRecord bodyPartRecord2 = enumerable.RandomElementWithFallback(null);
				if (bodyPartRecord2 == null)
				{
					base.FinalizeAndAddInjury(pawn, base.ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn), dinfo, ref result);
				}
				else
				{
					base.FinalizeAndAddInjury(pawn, base.ReduceDamageToPreserveOutsideParts(totalDamage * this.def.scratchSplitPercentage, dinfo, pawn), dinfo, ref result);
					DamageInfo dinfo3 = dinfo;
					dinfo3.SetHitPart(bodyPartRecord2);
					base.FinalizeAndAddInjury(pawn, base.ReduceDamageToPreserveOutsideParts(totalDamage * this.def.scratchSplitPercentage, dinfo3, pawn), dinfo3, ref result);
				}
			}
		}
	}
}
