using RimWorld;
using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class HediffGiver_Hypothermia : HediffGiver
	{
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float ambientTemperature = pawn.AmbientTemperature;
			FloatRange floatRange = pawn.ComfortableTemperatureRange();
			FloatRange floatRange2 = pawn.SafeTemperatureRange();
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(this.hediff, false);
			if (ambientTemperature < floatRange2.min)
			{
				float num = Mathf.Abs(ambientTemperature - floatRange2.min);
				float num2 = num * 6.45E-05f;
				num2 = Mathf.Max(num2, 0.00075f);
				HealthUtility.AdjustSeverity(pawn, this.hediff, num2);
				if (pawn.Dead)
				{
					return;
				}
			}
			if (firstHediffOfDef != null)
			{
				if (ambientTemperature > floatRange.min)
				{
					float num3 = firstHediffOfDef.Severity * 0.027f;
					num3 = Mathf.Clamp(num3, 0.0015f, 0.015f);
					firstHediffOfDef.Severity -= num3;
				}
				else if (ambientTemperature < 0f && firstHediffOfDef.Severity > 0.37f)
				{
					float num4 = 0.025f * firstHediffOfDef.Severity;
					if (Rand.Value < num4)
					{
						BodyPartRecord bodyPartRecord;
						if ((from x in pawn.RaceProps.body.AllPartsVulnerableToFrostbite
						where !hediffSet.PartIsMissing(x)
						select x).TryRandomElementByWeight((BodyPartRecord x) => x.def.frostbiteVulnerability, out bodyPartRecord))
						{
							int amount = Mathf.CeilToInt((float)bodyPartRecord.def.hitPoints * 0.5f);
							BodyPartRecord forceHitPart = bodyPartRecord;
							DamageInfo dinfo = new DamageInfo(DamageDefOf.Frostbite, amount, -1f, null, forceHitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
							pawn.TakeDamage(dinfo);
						}
					}
				}
			}
		}
	}
}
