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
			float num;
			if (!GenTemperature.TryGetTemperatureAtCellOrCaravanTile(pawn, out num))
			{
				return;
			}
			FloatRange floatRange = pawn.ComfortableTemperatureRange();
			FloatRange floatRange2 = pawn.SafeTemperatureRange();
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(this.hediff);
			if (num < floatRange2.min)
			{
				float num2 = Mathf.Abs(num - floatRange2.min);
				float num3 = num2 * 6.45E-05f;
				num3 = Mathf.Max(num3, 0.00075f);
				HealthUtility.AdjustSeverity(pawn, this.hediff, num3);
				if (pawn.Dead)
				{
					return;
				}
			}
			if (firstHediffOfDef != null)
			{
				if (num > floatRange.min)
				{
					float num4 = firstHediffOfDef.Severity * 0.027f;
					num4 = Mathf.Clamp(num4, 0.0015f, 0.015f);
					firstHediffOfDef.Severity -= num4;
				}
				else if (num < 0f && firstHediffOfDef.Severity > 0.37f)
				{
					float num5 = 0.025f * firstHediffOfDef.Severity;
					if (Rand.Value < num5)
					{
						BodyPartRecord bodyPartRecord;
						if ((from x in pawn.RaceProps.body.AllPartsVulnerableToFrostbite
						where !hediffSet.PartIsMissing(x)
						select x).TryRandomElementByWeight((BodyPartRecord x) => x.def.frostbiteVulnerability, out bodyPartRecord))
						{
							int amount = Mathf.CeilToInt((float)bodyPartRecord.def.hitPoints * 0.5f);
							BodyPartRecord forceHitPart = bodyPartRecord;
							DamageInfo dinfo = new DamageInfo(DamageDefOf.Frostbite, amount, -1f, null, forceHitPart, null);
							pawn.TakeDamage(dinfo);
						}
					}
				}
			}
		}
	}
}
