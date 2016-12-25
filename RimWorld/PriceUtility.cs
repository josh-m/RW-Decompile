using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PriceUtility
	{
		private const float SummaryHealthImpact = 0.8f;

		private const float CapacityImpact = 0.5f;

		private const float MissingCapacityFactor = 0.6f;

		private static readonly SimpleCurve AverageSkillCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.2f),
			new CurvePoint(5.5f, 1f),
			new CurvePoint(20f, 3f)
		};

		public static float PawnQualityPriceFactor(Pawn pawn)
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
					num *= Mathf.Lerp(0.5f, 1f, pawn.health.capacities.GetEfficiency(allDefsListForReading[i]));
				}
			}
			if (pawn.skills != null)
			{
				num *= PriceUtility.AverageSkillCurve.Evaluate(pawn.skills.skills.Average((SkillRecord sk) => (float)sk.Level));
			}
			return num * pawn.ageTracker.CurLifeStage.marketValueFactor;
		}
	}
}
