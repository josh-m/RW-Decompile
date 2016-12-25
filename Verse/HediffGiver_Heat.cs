using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class HediffGiver_Heat : HediffGiver
	{
		public static readonly SimpleCurve TemperatureOverageAdjustmentCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(25f, 25f),
			new CurvePoint(50f, 40f),
			new CurvePoint(100f, 60f),
			new CurvePoint(200f, 80f),
			new CurvePoint(400f, 100f),
			new CurvePoint(4000f, 1000f)
		};

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
			if (num > floatRange2.max)
			{
				float num2 = num - floatRange2.max;
				num2 = HediffGiver_Heat.TemperatureOverageAdjustmentCurve.Evaluate(num2);
				float num3 = num2 * 6.45E-05f;
				num3 = Mathf.Max(num3, 0.000375f);
				HealthUtility.AdjustSeverity(pawn, this.hediff, num3);
			}
			else if (firstHediffOfDef != null && num < floatRange.max)
			{
				float num4 = firstHediffOfDef.Severity * 0.027f;
				num4 = Mathf.Clamp(num4, 0.0015f, 0.015f);
				firstHediffOfDef.Severity -= num4;
			}
			if (pawn.Dead)
			{
				return;
			}
			if (pawn.HashOffsetTicks(60) % 7 == 0)
			{
				float num5 = floatRange.max + 150f;
				if (num > num5)
				{
					float num6 = num - num5;
					num6 = HediffGiver_Heat.TemperatureOverageAdjustmentCurve.Evaluate(num6);
					int amount = Mathf.Max(GenMath.RoundRandom(num6 * 0.06f), 3);
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, amount, -1f, null, null, null);
					dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
					pawn.TakeDamage(dinfo);
					if (pawn.Faction == Faction.OfPlayer)
					{
						Find.TickManager.slower.SignalForceNormalSpeed();
						if (MessagesRepeatAvoider.MessageShowAllowed("PawnBeingBurned", 60f))
						{
							Messages.Message("MessagePawnBeingBurned".Translate(new object[]
							{
								pawn.LabelShort
							}).CapitalizeFirst(), pawn, MessageSound.SeriousAlert);
						}
					}
				}
			}
		}
	}
}
