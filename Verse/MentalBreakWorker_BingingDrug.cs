using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class MentalBreakWorker_BingingDrug : MentalBreakWorker
	{
		public override float CommonalityFor(Pawn pawn)
		{
			int num = this.BingeableAddictionsCount(pawn);
			float num2;
			if (num == 0)
			{
				num2 = this.def.baseCommonality * 1f;
			}
			else
			{
				num2 = this.def.baseCommonality * 1.4f * (float)num;
			}
			if (pawn.story != null)
			{
				Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
				if (trait != null)
				{
					if (trait.Degree == 1)
					{
						num2 *= 2.5f;
					}
					else if (trait.Degree == 2)
					{
						num2 *= 5f;
					}
				}
			}
			return num2;
		}

		private int BingeableAddictionsCount(Pawn pawn)
		{
			int num = 0;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && AddictionUtility.CanBingeOnNow(pawn, hediff_Addiction.Chemical, DrugCategory.Any))
				{
					num++;
				}
			}
			return num;
		}
	}
}
