using System;
using UnityEngine;

namespace Verse
{
	public struct DamageInfo
	{
		private DamageDef defInt;

		private int amountInt;

		private Thing instigatorInt;

		private float angleInt;

		private BodyPartDamageInfo? partInt;

		private ThingDef sourceInt;

		private bool instantOldInjuryInt;

		private BodyPartGroupDef linkedBodyPartGroupInt;

		private HediffDef linkedHediffDefInt;

		private bool allowDamagePropagationInt;

		public DamageDef Def
		{
			get
			{
				return this.defInt;
			}
		}

		public float Angle
		{
			get
			{
				return this.angleInt;
			}
		}

		public Thing Instigator
		{
			get
			{
				return this.instigatorInt;
			}
		}

		public BodyPartDamageInfo? Part
		{
			get
			{
				return this.partInt;
			}
		}

		public ThingDef Source
		{
			get
			{
				return this.sourceInt;
			}
		}

		public bool InstantOldInjury
		{
			get
			{
				return this.instantOldInjuryInt;
			}
		}

		public BodyPartGroupDef LinkedBodyPartGroup
		{
			get
			{
				return this.linkedBodyPartGroupInt;
			}
		}

		public HediffDef LinkedHediffDef
		{
			get
			{
				return this.linkedHediffDefInt;
			}
		}

		public bool AllowDamagePropagation
		{
			get
			{
				return !this.InstantOldInjury && this.allowDamagePropagationInt;
			}
			set
			{
				this.allowDamagePropagationInt = value;
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

		public DamageInfo(DamageDef def, int amount, Thing instigator = null, BodyPartDamageInfo? part = null, ThingDef source = null)
		{
			this.defInt = def;
			this.amountInt = amount;
			this.instigatorInt = instigator;
			this.angleInt = (float)Rand.RangeInclusive(0, 359);
			this.partInt = part;
			this.sourceInt = source;
			this.instantOldInjuryInt = false;
			this.linkedBodyPartGroupInt = null;
			this.linkedHediffDefInt = null;
			this.allowDamagePropagationInt = true;
		}

		public DamageInfo(DamageDef def, int amount, Thing instigator, float direction, BodyPartDamageInfo? part = null, ThingDef source = null)
		{
			this.defInt = def;
			this.amountInt = amount;
			this.instigatorInt = instigator;
			this.angleInt = direction;
			this.partInt = part;
			this.sourceInt = source;
			this.instantOldInjuryInt = false;
			this.linkedBodyPartGroupInt = null;
			this.linkedHediffDefInt = null;
			this.allowDamagePropagationInt = true;
		}

		public DamageInfo(DamageDef def, int amount, Thing instigator, Vector3 direction, BodyPartDamageInfo? part = null, ThingDef source = null)
		{
			this.defInt = def;
			this.amountInt = amount;
			this.instigatorInt = instigator;
			this.partInt = part;
			this.sourceInt = source;
			this.instantOldInjuryInt = false;
			this.linkedBodyPartGroupInt = null;
			this.linkedHediffDefInt = null;
			this.allowDamagePropagationInt = true;
			if (direction.x != 0f || direction.z != 0f)
			{
				this.angleInt = Quaternion.LookRotation(direction).eulerAngles.y;
			}
			else
			{
				this.angleInt = (float)Rand.RangeInclusive(0, 359);
			}
		}

		public DamageInfo(DamageInfo cloneSource)
		{
			this.defInt = cloneSource.defInt;
			this.amountInt = cloneSource.amountInt;
			this.instigatorInt = cloneSource.instigatorInt;
			this.angleInt = cloneSource.angleInt;
			this.partInt = cloneSource.partInt;
			this.sourceInt = cloneSource.sourceInt;
			this.instantOldInjuryInt = cloneSource.instantOldInjuryInt;
			this.linkedBodyPartGroupInt = cloneSource.linkedBodyPartGroupInt;
			this.linkedHediffDefInt = cloneSource.linkedHediffDefInt;
			this.allowDamagePropagationInt = true;
		}

		public void SetAmount(int newAmount)
		{
			this.amountInt = newAmount;
		}

		public void SetPart(BodyPartDamageInfo info)
		{
			this.partInt = new BodyPartDamageInfo?(info);
		}

		public void SetInstantOldInjury(bool val)
		{
			this.instantOldInjuryInt = val;
		}

		public void SetLinkedBodyPartGroup(BodyPartGroupDef gr)
		{
			this.linkedBodyPartGroupInt = gr;
		}

		public void SetLinkedHediffDef(HediffDef hd)
		{
			this.linkedHediffDefInt = hd;
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
				this.instigatorInt,
				", dir=",
				this.angleInt.ToString("####.0"),
				")"
			});
		}
	}
}
