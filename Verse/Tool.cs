using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class Tool
	{
		public string id;

		public string label;

		public bool labelUsedInLogging = true;

		public List<ToolCapacityDef> capacities;

		public float power;

		public float cooldownTime;

		public SurpriseAttackProps surpriseAttack;

		public float commonality = 1f;

		public bool alwaysTreatAsWeapon;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public string Id
		{
			get
			{
				if (!this.id.NullOrEmpty())
				{
					return this.id;
				}
				return this.label;
			}
		}

		public float AdjustedMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
		{
			float num = this.power;
			if (ownerEquipment != null)
			{
				num *= ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, true);
				if (ownerEquipment.Stuff != null && damageDef != null)
				{
					num *= ownerEquipment.Stuff.GetStatValueAbstract(damageDef.armorCategory.multStat, null);
				}
			}
			return num;
		}

		public float AdjustedCooldown(Thing ownerEquipment)
		{
			return this.cooldownTime * ((ownerEquipment != null) ? ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_CooldownMultiplier, true) : 1f);
		}

		public override string ToString()
		{
			return this.label;
		}
	}
}
