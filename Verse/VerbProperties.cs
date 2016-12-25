using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class VerbProperties
	{
		private enum RangeCategory : byte
		{
			Touch,
			Short,
			Medium,
			Long
		}

		private const float DistTouch = 4f;

		private const float DistShort = 15f;

		private const float DistMedium = 30f;

		private const float DistLong = 50f;

		public VerbCategory category = VerbCategory.Nonnative;

		public Type verbClass;

		public string label;

		public bool isPrimary = true;

		public float minRange;

		public float range = 1f;

		public int burstShotCount = 1;

		public int ticksBetweenBurstShots = 15;

		public float noiseRadius = 3f;

		public bool hasStandardCommand;

		public bool targetable = true;

		public TargetingParameters targetParams = new TargetingParameters();

		public bool requireLineOfSight = true;

		public bool mustCastOnOpenGround;

		public bool forceNormalTimeSpeed = true;

		public bool onlyManualCast;

		public bool stopBurstWithoutLos = true;

		public SurpriseAttackProps surpriseAttack;

		public float warmupTime;

		public float defaultCooldownTime;

		public SoundDef soundCast;

		public SoundDef soundCastTail;

		public float muzzleFlashScale;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public DamageDef meleeDamageDef;

		public int meleeDamageBaseAmount = 1;

		public bool ai_IsWeapon = true;

		public bool ai_IsIncendiary;

		public bool ai_IsBuildingDestroyer;

		public ThingDef projectileDef;

		public float forcedMissRadius;

		public float accuracyTouch = 1f;

		public float accuracyShort = 1f;

		public float accuracyMedium = 1f;

		public float accuracyLong = 1f;

		public bool MeleeRange
		{
			get
			{
				return this.range < 1.1f;
			}
		}

		public bool NeedsLineOfSight
		{
			get
			{
				return !this.projectileDef.projectile.flyOverhead;
			}
		}

		public bool CausesTimeSlowdown
		{
			get
			{
				return this.ai_IsWeapon && this.forceNormalTimeSpeed;
			}
		}

		public string AccuracySummaryString
		{
			get
			{
				return string.Concat(new string[]
				{
					this.accuracyTouch.ToStringPercent(),
					" - ",
					this.accuracyShort.ToStringPercent(),
					" - ",
					this.accuracyMedium.ToStringPercent(),
					" - ",
					this.accuracyLong.ToStringPercent()
				});
			}
		}

		public int AdjustedMeleeDamageAmount(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			float num;
			if (ownerVerb.ownerEquipment != null)
			{
				num = ownerVerb.ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageAmount, true);
			}
			else
			{
				num = (float)this.meleeDamageBaseAmount;
			}
			if (attacker != null)
			{
				num *= ownerVerb.GetDamageFactorFor(attacker);
			}
			return Mathf.Max(1, Mathf.RoundToInt(num));
		}

		public int AdjustedCooldownTicks(Thing equipment)
		{
			if (equipment == null)
			{
				return this.defaultCooldownTime.SecondsToTicks();
			}
			if (this.MeleeRange)
			{
				return equipment.GetStatValue(StatDefOf.MeleeWeapon_Cooldown, true).SecondsToTicks();
			}
			return equipment.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true).SecondsToTicks();
		}

		private float AdjustedAccuracy(VerbProperties.RangeCategory cat, Thing equipment)
		{
			if (equipment != null)
			{
				StatDef stat = null;
				switch (cat)
				{
				case VerbProperties.RangeCategory.Touch:
					stat = StatDefOf.AccuracyTouch;
					break;
				case VerbProperties.RangeCategory.Short:
					stat = StatDefOf.AccuracyShort;
					break;
				case VerbProperties.RangeCategory.Medium:
					stat = StatDefOf.AccuracyMedium;
					break;
				case VerbProperties.RangeCategory.Long:
					stat = StatDefOf.AccuracyLong;
					break;
				}
				return equipment.GetStatValue(stat, true);
			}
			switch (cat)
			{
			case VerbProperties.RangeCategory.Touch:
				return this.accuracyTouch;
			case VerbProperties.RangeCategory.Short:
				return this.accuracyShort;
			case VerbProperties.RangeCategory.Medium:
				return this.accuracyMedium;
			case VerbProperties.RangeCategory.Long:
				return this.accuracyLong;
			default:
				throw new InvalidOperationException();
			}
		}

		public float GetHitChanceFactor(Thing equipment, float dist)
		{
			float num;
			if (dist <= 4f)
			{
				num = this.AdjustedAccuracy(VerbProperties.RangeCategory.Touch, equipment);
			}
			else if (dist <= 15f)
			{
				num = Mathf.Lerp(this.AdjustedAccuracy(VerbProperties.RangeCategory.Touch, equipment), this.AdjustedAccuracy(VerbProperties.RangeCategory.Short, equipment), (dist - 4f) / 11f);
			}
			else if (dist <= 30f)
			{
				num = Mathf.Lerp(this.AdjustedAccuracy(VerbProperties.RangeCategory.Short, equipment), this.AdjustedAccuracy(VerbProperties.RangeCategory.Medium, equipment), (dist - 15f) / 15f);
			}
			else if (dist <= 50f)
			{
				num = Mathf.Lerp(this.AdjustedAccuracy(VerbProperties.RangeCategory.Medium, equipment), this.AdjustedAccuracy(VerbProperties.RangeCategory.Long, equipment), (dist - 30f) / 20f);
			}
			else
			{
				num = this.AdjustedAccuracy(VerbProperties.RangeCategory.Long, equipment);
			}
			if (num < 0.01f)
			{
				num = 0.01f;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}

		public override string ToString()
		{
			string str;
			if (!this.label.NullOrEmpty())
			{
				str = this.label;
			}
			else
			{
				str = string.Concat(new object[]
				{
					"range=",
					this.range,
					", projectile=",
					(this.projectileDef == null) ? "null" : this.projectileDef.defName
				});
			}
			return "VerbProperties(" + str + ")";
		}
	}
}
