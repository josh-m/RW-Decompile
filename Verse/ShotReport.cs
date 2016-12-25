using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public struct ShotReport
	{
		public const float LayingDownHitChanceFactorMinDistance = 4.5f;

		public const float HitChanceFactorIfLayingDown = 0.2f;

		private const float NonPawnShooterHitFactorPerDistance = 0.96f;

		private const float DarknessFactor = 0.85f;

		private const float ExecutionMaxDistance = 3.9f;

		private const float ExecutionFactor = 7.5f;

		private TargetInfo target;

		private float distance;

		private List<CoverInfo> covers;

		private float coversOverallBlockChance;

		private float factorFromShooterAndDist;

		private float factorFromEquipment;

		private float factorFromTargetSize;

		private float factorFromWeather;

		private float factorFromSightEfficiency;

		private PsychGlow targetLighting;

		private float forcedMissRadius;

		private float FactorFromDarkness
		{
			get
			{
				if (this.targetLighting == PsychGlow.Dark)
				{
					return 0.85f;
				}
				return 1f;
			}
		}

		private float FactorFromPosture
		{
			get
			{
				if (this.target.HasThing)
				{
					Pawn pawn = this.target.Thing as Pawn;
					if (pawn != null && this.distance >= 4.5f && pawn.GetPosture() != PawnPosture.Standing)
					{
						return 0.2f;
					}
				}
				return 1f;
			}
		}

		private float FactorFromExecution
		{
			get
			{
				if (this.target.HasThing)
				{
					Pawn pawn = this.target.Thing as Pawn;
					if (pawn != null && this.distance <= 3.9f && pawn.GetPosture() != PawnPosture.Standing)
					{
						return 7.5f;
					}
				}
				return 1f;
			}
		}

		public float ChanceToNotHitCover
		{
			get
			{
				return 1f - this.coversOverallBlockChance;
			}
		}

		public float ChanceToNotGoWild_IgnoringPosture
		{
			get
			{
				return this.factorFromShooterAndDist * this.factorFromEquipment * this.factorFromWeather * this.factorFromTargetSize * this.factorFromSightEfficiency * this.FactorFromDarkness * this.FactorFromExecution;
			}
		}

		public float TotalEstimatedHitChance
		{
			get
			{
				float value = this.ChanceToNotGoWild_IgnoringPosture * this.FactorFromPosture * this.ChanceToNotHitCover;
				return Mathf.Clamp01(value);
			}
		}

		public static ShotReport HitReportFor(Thing caster, Verb verb, TargetInfo target)
		{
			Pawn pawn = caster as Pawn;
			IntVec3 cell = target.Cell;
			ShotReport result;
			result.distance = (cell - caster.Position).LengthHorizontal;
			result.target = target;
			float f;
			if (pawn != null)
			{
				f = pawn.GetStatValue(StatDefOf.ShootingAccuracy, true);
			}
			else
			{
				f = 0.96f;
			}
			result.factorFromShooterAndDist = Mathf.Pow(f, result.distance);
			if (result.factorFromShooterAndDist < 0.0201f)
			{
				result.factorFromShooterAndDist = 0.0201f;
			}
			if (pawn != null)
			{
				result.factorFromSightEfficiency = pawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight);
			}
			else
			{
				result.factorFromSightEfficiency = 1f;
			}
			result.factorFromEquipment = verb.verbProps.GetHitChanceFactor(verb.ownerEquipment, result.distance);
			result.covers = CoverUtility.CalculateCoverGiverSet(cell, caster.Position);
			result.coversOverallBlockChance = CoverUtility.CalculateOverallBlockChance(cell, caster.Position);
			if (!caster.Position.Roofed() && !target.Cell.Roofed())
			{
				result.factorFromWeather = Find.WeatherManager.CurWeatherAccuracyMultiplier;
			}
			else
			{
				result.factorFromWeather = 1f;
			}
			result.factorFromTargetSize = 1f;
			if (target.HasThing)
			{
				Pawn pawn2 = target.Thing as Pawn;
				if (pawn2 != null)
				{
					result.factorFromTargetSize = pawn2.BodySize;
				}
				else
				{
					result.factorFromTargetSize = target.Thing.def.fillPercent * 1.7f;
				}
				result.factorFromTargetSize = Mathf.Clamp(result.factorFromTargetSize, 0.5f, 2f);
			}
			result.targetLighting = Find.GlowGrid.PsychGlowAt(cell);
			result.forcedMissRadius = verb.verbProps.forcedMissRadius;
			return result;
		}

		public string GetTextReadout()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.forcedMissRadius > 0.5f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("WeaponMissRadius".Translate() + "   " + this.forcedMissRadius.ToString("F1"));
			}
			else
			{
				stringBuilder.AppendLine(" " + this.TotalEstimatedHitChance.ToStringPercent());
				stringBuilder.AppendLine("   " + "ShootReportShooterAbility".Translate() + "  " + this.factorFromShooterAndDist.ToStringPercent());
				if (this.factorFromEquipment < 0.99f)
				{
					stringBuilder.AppendLine("   " + "ShootReportWeapon".Translate() + "        " + this.factorFromEquipment.ToStringPercent());
				}
				if (this.target.HasThing && this.factorFromTargetSize != 1f)
				{
					stringBuilder.AppendLine("   " + "TargetSize".Translate() + "       " + this.factorFromTargetSize.ToStringPercent());
				}
				if (this.factorFromWeather < 0.99f)
				{
					stringBuilder.AppendLine("   " + "Weather".Translate() + "         " + this.factorFromWeather.ToStringPercent());
				}
				if (this.FactorFromDarkness < 0.99999f)
				{
					stringBuilder.AppendLine("   " + "Darkness".Translate() + "       " + this.FactorFromDarkness.ToStringPercent());
				}
				if (this.FactorFromPosture < 0.9999f)
				{
					stringBuilder.AppendLine("   " + "TargetProne".Translate() + "  " + this.FactorFromPosture.ToStringPercent());
				}
				if (this.FactorFromExecution != 1f)
				{
					stringBuilder.AppendLine("   " + "Execution".Translate() + "   " + this.FactorFromExecution.ToStringPercent());
				}
				if (this.ChanceToNotHitCover < 1f)
				{
					stringBuilder.AppendLine("   " + "ShootingCover".Translate() + "        " + this.ChanceToNotHitCover.ToStringPercent());
					for (int i = 0; i < this.covers.Count; i++)
					{
						CoverInfo coverInfo = this.covers[i];
						stringBuilder.AppendLine("     " + "CoverThingBlocksPercentOfShots".Translate(new object[]
						{
							coverInfo.Thing.LabelCap,
							coverInfo.BlockChance.ToStringPercent()
						}));
					}
				}
				else
				{
					stringBuilder.AppendLine("   (" + "NoCoverLower".Translate() + ")");
				}
			}
			return stringBuilder.ToString();
		}

		public Thing GetRandomCoverToMissInto()
		{
			return this.covers.RandomElementByWeight((CoverInfo c) => c.BlockChance).Thing;
		}
	}
}
