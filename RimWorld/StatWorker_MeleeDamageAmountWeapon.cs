using System;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDamageAmountWeapon : StatWorker_MeleeDamageAmount
	{
		public override bool ShouldShowFor(BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			return thingDef != null && thingDef.IsMeleeWeapon;
		}

		protected override DamageArmorCategory CategoryOfDamage(ThingDef def)
		{
			return def.Verbs[0].meleeDamageDef.armorCategory;
		}
	}
}
