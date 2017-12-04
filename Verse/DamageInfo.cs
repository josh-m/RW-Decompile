using System;
using UnityEngine;

namespace Verse
{
	public struct DamageInfo
	{
		public enum SourceCategory
		{
			ThingOrUnknown,
			Collapse
		}

		private DamageDef defInt;

		private int amountInt;

		private float angleInt;

		private Thing instigatorInt;

		private DamageInfo.SourceCategory categoryInt;

		private BodyPartRecord hitPartInt;

		private BodyPartHeight heightInt;

		private BodyPartDepth depthInt;

		private ThingDef weaponInt;

		private BodyPartGroupDef weaponBodyPartGroupInt;

		private HediffDef weaponHediffInt;

		private bool instantOldInjuryInt;

		private bool allowDamagePropagationInt;

		public DamageDef Def
		{
			get
			{
				return this.defInt;
			}
		}

		public int Amount
		{
			get
			{
				if (!DebugSettings.enableDamage)
				{
					return 0;
				}
				return this.amountInt;
			}
		}

		public Thing Instigator
		{
			get
			{
				return this.instigatorInt;
			}
		}

		public DamageInfo.SourceCategory Category
		{
			get
			{
				return this.categoryInt;
			}
		}

		public float Angle
		{
			get
			{
				return this.angleInt;
			}
		}

		public BodyPartRecord HitPart
		{
			get
			{
				return this.hitPartInt;
			}
		}

		public BodyPartHeight Height
		{
			get
			{
				return this.heightInt;
			}
		}

		public BodyPartDepth Depth
		{
			get
			{
				return this.depthInt;
			}
		}

		public ThingDef Weapon
		{
			get
			{
				return this.weaponInt;
			}
		}

		public BodyPartGroupDef WeaponBodyPartGroup
		{
			get
			{
				return this.weaponBodyPartGroupInt;
			}
		}

		public HediffDef WeaponLinkedHediff
		{
			get
			{
				return this.weaponHediffInt;
			}
		}

		public bool InstantOldInjury
		{
			get
			{
				return this.instantOldInjuryInt;
			}
		}

		public bool AllowDamagePropagation
		{
			get
			{
				return !this.InstantOldInjury && this.allowDamagePropagationInt;
			}
		}

		public DamageInfo(DamageDef def, int amount, float angle = -1f, Thing instigator = null, BodyPartRecord hitPart = null, ThingDef weapon = null, DamageInfo.SourceCategory category = DamageInfo.SourceCategory.ThingOrUnknown)
		{
			this.defInt = def;
			this.amountInt = amount;
			if (angle < 0f)
			{
				this.angleInt = (float)Rand.RangeInclusive(0, 359);
			}
			else
			{
				this.angleInt = angle;
			}
			this.instigatorInt = instigator;
			this.categoryInt = category;
			this.hitPartInt = hitPart;
			this.heightInt = BodyPartHeight.Undefined;
			this.depthInt = BodyPartDepth.Undefined;
			this.weaponInt = weapon;
			this.weaponBodyPartGroupInt = null;
			this.weaponHediffInt = null;
			this.instantOldInjuryInt = false;
			this.allowDamagePropagationInt = true;
		}

		public DamageInfo(DamageInfo cloneSource)
		{
			this.defInt = cloneSource.defInt;
			this.amountInt = cloneSource.amountInt;
			this.angleInt = cloneSource.angleInt;
			this.instigatorInt = cloneSource.instigatorInt;
			this.categoryInt = cloneSource.categoryInt;
			this.hitPartInt = cloneSource.hitPartInt;
			this.heightInt = cloneSource.heightInt;
			this.depthInt = cloneSource.depthInt;
			this.weaponInt = cloneSource.weaponInt;
			this.weaponBodyPartGroupInt = cloneSource.weaponBodyPartGroupInt;
			this.weaponHediffInt = cloneSource.weaponHediffInt;
			this.instantOldInjuryInt = cloneSource.instantOldInjuryInt;
			this.allowDamagePropagationInt = cloneSource.allowDamagePropagationInt;
		}

		public void SetAmount(int newAmount)
		{
			this.amountInt = newAmount;
		}

		public void SetBodyRegion(BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined)
		{
			this.heightInt = height;
			this.depthInt = depth;
		}

		public void SetHitPart(BodyPartRecord forceHitPart)
		{
			this.hitPartInt = forceHitPart;
		}

		public void SetInstantOldInjury(bool val)
		{
			this.instantOldInjuryInt = val;
		}

		public void SetWeaponBodyPartGroup(BodyPartGroupDef gr)
		{
			this.weaponBodyPartGroupInt = gr;
		}

		public void SetWeaponHediff(HediffDef hd)
		{
			this.weaponHediffInt = hd;
		}

		public void SetAllowDamagePropagation(bool val)
		{
			this.allowDamagePropagationInt = val;
		}

		public void SetAngle(Vector3 vec)
		{
			if (vec.x != 0f || vec.z != 0f)
			{
				this.angleInt = Quaternion.LookRotation(vec).eulerAngles.y;
			}
			else
			{
				this.angleInt = (float)Rand.RangeInclusive(0, 359);
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(def=",
				this.defInt,
				", amount= ",
				this.amountInt,
				", instigator=",
				(this.instigatorInt == null) ? this.categoryInt.ToString() : this.instigatorInt.ToString(),
				", angle=",
				this.angleInt.ToString("F1"),
				")"
			});
		}
	}
}
