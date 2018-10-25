using RimWorld;
using System;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
	public class HediffGiver_Heat : HediffGiver
	{
		private const int BurnCheckInterval = 420;

		public static readonly string MemoPawnBurnedByAir = "PawnBurnedByAir";

		public static readonly SimpleCurve TemperatureOverageAdjustmentCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(25f, 25f),
				true
			},
			{
				new CurvePoint(50f, 40f),
				true
			},
			{
				new CurvePoint(100f, 60f),
				true
			},
			{
				new CurvePoint(200f, 80f),
				true
			},
			{
				new CurvePoint(400f, 100f),
				true
			},
			{
				new CurvePoint(4000f, 1000f),
				true
			}
		};

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float ambientTemperature = pawn.AmbientTemperature;
			FloatRange floatRange = pawn.ComfortableTemperatureRange();
			FloatRange floatRange2 = pawn.SafeTemperatureRange();
			HediffSet hediffSet = pawn.health.hediffSet;
			Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(this.hediff, false);
			if (ambientTemperature > floatRange2.max)
			{
				float num = ambientTemperature - floatRange2.max;
				num = HediffGiver_Heat.TemperatureOverageAdjustmentCurve.Evaluate(num);
				float num2 = num * 6.45E-05f;
				num2 = Mathf.Max(num2, 0.000375f);
				HealthUtility.AdjustSeverity(pawn, this.hediff, num2);
			}
			else if (firstHediffOfDef != null && ambientTemperature < floatRange.max)
			{
				float num3 = firstHediffOfDef.Severity * 0.027f;
				num3 = Mathf.Clamp(num3, 0.0015f, 0.015f);
				firstHediffOfDef.Severity -= num3;
			}
			if (pawn.Dead)
			{
				return;
			}
			if (pawn.IsNestedHashIntervalTick(60, 420))
			{
				float num4 = floatRange.max + 150f;
				if (ambientTemperature > num4)
				{
					float num5 = ambientTemperature - num4;
					num5 = HediffGiver_Heat.TemperatureOverageAdjustmentCurve.Evaluate(num5);
					int num6 = Mathf.Max(GenMath.RoundRandom(num5 * 0.06f), 3);
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, (float)num6, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
					dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
					pawn.TakeDamage(dinfo);
					if (pawn.Faction == Faction.OfPlayer)
					{
						Find.TickManager.slower.SignalForceNormalSpeed();
						if (MessagesRepeatAvoider.MessageShowAllowed("PawnBeingBurned", 60f))
						{
							Messages.Message("MessagePawnBeingBurned".Translate(pawn.LabelShort, pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall, true);
						}
					}
					Lord lord = pawn.GetLord();
					if (lord != null)
					{
						lord.ReceiveMemo(HediffGiver_Heat.MemoPawnBurnedByAir);
					}
				}
			}
		}
	}
}
