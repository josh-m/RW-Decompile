using System;
using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonSmeltableWeapon : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return t.def.thingCategories.Contains(ThingCategoryDefOf.Weapons) && !t.Smeltable;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			return def.thingCategories.Contains(ThingCategoryDefOf.Weapons) && !def.MadeFromStuff && def.smeltProducts == null;
		}
	}
}
