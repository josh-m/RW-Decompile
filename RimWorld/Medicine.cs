using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Medicine : ThingWithComps
	{
		private static HashSet<HediffDef> hediffsHealedTogether = new HashSet<HediffDef>();

		public static int GetMedicineCountToFullyHeal(Pawn pawn)
		{
			int num = 0;
			int num2 = 0;
			foreach (Hediff_Injury current in from x in pawn.health.hediffSet.GetInjuriesTendable()
			orderby x.Severity descending
			select x)
			{
				int num3 = Mathf.Min(Mathf.RoundToInt(current.Severity), 20);
				if (num2 + num3 > 20)
				{
					num2 = num3;
					num++;
				}
				else
				{
					num2 += num3;
				}
			}
			if (num2 != 0)
			{
				num++;
			}
			List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
			for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
			{
				if (missingPartsCommonAncestors[i].IsFresh)
				{
					num++;
				}
			}
			Medicine.hediffsHealedTogether.Clear();
			foreach (Hediff current2 in pawn.health.hediffSet.GetTendableNonInjuryNonMissingPartHediffs())
			{
				HediffCompProperties_TendDuration hediffCompProperties_TendDuration = current2.def.CompProps<HediffCompProperties_TendDuration>();
				if (hediffCompProperties_TendDuration != null && hediffCompProperties_TendDuration.tendAllAtOnce)
				{
					if (Medicine.hediffsHealedTogether.Add(current2.def))
					{
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			Medicine.hediffsHealedTogether.Clear();
			return num;
		}
	}
}
