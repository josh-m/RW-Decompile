using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PriceUtility
	{
		private const float MinFactor = 0.1f;

		private const float SummaryHealthImpact = 0.8f;

		private const float CapacityImpact = 0.5f;

		private const float MissingCapacityFactor = 0.6f;

		private static readonly SimpleCurve AverageSkillCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.2f),
				true
			},
			{
				new CurvePoint(5.5f, 1f),
				true
			},
			{
				new CurvePoint(20f, 3f),
				true
			}
		};

		public static float PawnQualityPriceFactor(Pawn pawn, StringBuilder explanation = null)
		{
			float num = 1f;
			num *= Mathf.Lerp(0.199999988f, 1f, pawn.health.summaryHealth.SummaryHealthPercent);
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (!pawn.health.capacities.CapableOf(allDefsListForReading[i]))
				{
					num *= 0.6f;
				}
				else
				{
					num *= Mathf.Lerp(0.5f, 1f, pawn.health.capacities.GetLevel(allDefsListForReading[i]));
				}
			}
			if (pawn.skills != null)
			{
				num *= PriceUtility.AverageSkillCurve.Evaluate(pawn.skills.skills.Average((SkillRecord sk) => (float)sk.Level));
			}
			num *= pawn.ageTracker.CurLifeStage.marketValueFactor;
			if (pawn.story != null && pawn.story.traits != null)
			{
				for (int j = 0; j < pawn.story.traits.allTraits.Count; j++)
				{
					Trait trait = pawn.story.traits.allTraits[j];
					num += trait.CurrentData.marketValueFactorOffset;
				}
			}
			if (num < 0.1f)
			{
				num = 0.1f;
			}
			if (explanation != null)
			{
				if (explanation.Length > 0)
				{
					explanation.AppendLine();
				}
				explanation.Append("StatsReport_CharacterQuality".Translate() + ": x" + num.ToStringPercent());
			}
			return num;
		}

		public static float PawnQualityPriceOffset(Pawn pawn, StringBuilder explanation = null)
		{
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_AddedPart && hediffs[i].def.spawnThingOnRemoved != null)
				{
					float baseMarketValue = hediffs[i].def.spawnThingOnRemoved.BaseMarketValue;
					if (baseMarketValue >= 1f)
					{
						num += baseMarketValue;
						if (explanation != null)
						{
							if (explanation.Length > 0)
							{
								explanation.AppendLine();
							}
							explanation.Append(hediffs[i].LabelBaseCap + ": " + baseMarketValue.ToStringMoneyOffset(null));
						}
					}
				}
			}
			return num;
		}
	}
}
