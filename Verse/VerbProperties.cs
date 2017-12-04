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

		public VerbCategory category = VerbCategory.Nonnative;

		public Type verbClass = typeof(Verb);

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

		public float commonality = 1f;

		public float warmupTime;

		public float defaultCooldownTime;

		public SoundDef soundCast;

		public SoundDef soundCastTail;

		public SoundDef soundAiming;

		public float muzzleFlashScale;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public DamageDef meleeDamageDef;

		public int meleeDamageBaseAmount = 1;

		public bool ai_IsWeapon = true;

		public bool ai_IsBuildingDestroyer;

		public float ai_AvoidFriendlyFireRadius;

		public ThingDef defaultProjectile;

		public float forcedMissRadius;

		public float accuracyTouch = 1f;

		public float accuracyShort = 1f;

		public float accuracyMedium = 1f;

		public float accuracyLong = 1f;

		public bool meleeShoot;

		private const float DistTouch = 4f;

		private const float DistShort = 15f;

		private const float DistMedium = 30f;

		private const float DistLong = 50f;

		private const float MeleeGunfireWeighting = 0.25f;

		private const float BodypartVerbWeighting = 0.3f;

		public bool MeleeRange
		{
			get
			{
				return this.range < 1.1f;
			}
		}

		public bool CausesTimeSlowdown
		{
			get
			{
				return this.ai_IsWeapon && this.forceNormalTimeSpeed;
			}
		}

		public bool LaunchesProjectile
		{
			get
			{
				return typeof(Verb_LaunchProjectile).IsAssignableFrom(this.verbClass);
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

		public bool IsMeleeAttack
		{
			get
			{
				return typeof(Verb_MeleeAttack).IsAssignableFrom(this.verbClass);
			}
		}

		public float BaseMeleeSelectionWeight
		{
			get
			{
				return this.AdjustedMeleeSelectionWeight(null, null, null);
			}
		}

		public float AdjustedMeleeDamageAmount(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			if ((ownerVerb == null) ? (!typeof(Verb_MeleeAttack).IsAssignableFrom(this.verbClass)) : (!(ownerVerb is Verb_MeleeAttack)))
			{
				Log.ErrorOnce(string.Format("Attempting to get melee damage for a non-melee verb {0}", this), 26181238);
			}
			float num;
			if (ownerVerb != null && ownerVerb.tool != null)
			{
				num = ownerVerb.tool.AdjustedMeleeDamageAmount(ownerVerb.ownerEquipment, this.meleeDamageDef);
			}
			else
			{
				num = (float)this.meleeDamageBaseAmount;
			}
			if (attacker != null)
			{
				num *= ownerVerb.GetDamageFactorFor(attacker);
			}
			return num;
		}

		private float AdjustedExpectedMeleeDamage(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			if (this.IsMeleeAttack)
			{
				return this.AdjustedMeleeDamageAmount(ownerVerb, attacker, equipment);
			}
			if (this.LaunchesProjectile && this.defaultProjectile != null)
			{
				return (float)this.defaultProjectile.projectile.damageAmountBase;
			}
			return 0f;
		}

		public float AdjustedMeleeSelectionWeight(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			float num = this.AdjustedExpectedMeleeDamage(ownerVerb, attacker, equipment) * this.commonality * ((ownerVerb.tool != null) ? ownerVerb.tool.commonality : 1f);
			if (this.IsMeleeAttack && equipment != null)
			{
				return num;
			}
			if (this.IsMeleeAttack && ownerVerb.tool != null && ownerVerb.tool.alwaysTreatAsWeapon)
			{
				return num;
			}
			if (this.IsMeleeAttack)
			{
				return num * 0.3f;
			}
			if (this.meleeShoot)
			{
				return num * 0.25f / equipment.GetStatValue(StatDefOf.Weapon_Bulk, true);
			}
			return 0f;
		}

		public float AdjustedCooldown(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			if (ownerVerb.tool != null)
			{
				return ownerVerb.tool.AdjustedCooldown(equipment);
			}
			if (equipment != null && !this.MeleeRange)
			{
				return equipment.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true);
			}
			return this.defaultCooldownTime;
		}

		public int AdjustedCooldownTicks(Verb ownerVerb, Pawn attacker, Thing equipment)
		{
			return this.AdjustedCooldown(ownerVerb, attacker, equipment).SecondsToTicks();
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
					", defaultProjectile=",
					this.defaultProjectile.ToStringSafe<ThingDef>()
				});
			}
			return "VerbProperties(" + str + ")";
		}

		public new VerbProperties MemberwiseClone()
		{
			return (VerbProperties)base.MemberwiseClone();
		}
	}
}
