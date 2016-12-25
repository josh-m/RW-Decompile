using System;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDamageAmountTrap : StatWorker_MeleeDamageAmount
	{
		public override bool ShouldShowFor(BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			return thingDef != null && thingDef.category == ThingCategory.Building && thingDef.building.isTrap;
		}

		protected override DamageArmorCategory CategoryOfDamage(ThingDef def)
		{
			return def.building.trapDamageCategory;
		}
	}
}
