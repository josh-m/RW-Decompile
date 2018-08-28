using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public class Tool
	{
		[Unsaved]
		public string id;

		[MustTranslate]
		public string label;

		[TranslationHandle, Unsaved]
		public string untranslatedLabel;

		public bool labelUsedInLogging = true;

		public List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();

		public float power;

		public float armorPenetration = -1f;

		public float cooldownTime;

		public SurpriseAttackProps surpriseAttack;

		public HediffDef hediff;

		public float chanceFactor = 1f;

		public bool alwaysTreatAsWeapon;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public bool ensureLinkedBodyPartsGroupAlwaysUsable;

		public string LabelCap
		{
			get
			{
				return this.label.CapitalizeFirst();
			}
		}

		public IEnumerable<ManeuverDef> Maneuvers
		{
			get
			{
				return from x in DefDatabase<ManeuverDef>.AllDefsListForReading
				where this.capacities.Contains(x.requiredCapacity)
				select x;
			}
		}

		public IEnumerable<VerbProperties> VerbsProperties
		{
			get
			{
				return from x in this.Maneuvers
				select x.verb;
			}
		}

		public float AdjustedBaseMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
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

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors()
		{
			if (this.id.NullOrEmpty())
			{
				yield return "tool has null id (power=" + this.power.ToString("0.##") + ")";
			}
		}
	}
}
