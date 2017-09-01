using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Weapons : ItemCollectionGenerator_Standard
	{
		private static List<ThingDef> weapons = new List<ThingDef>();

		public static void Reset()
		{
			ItemCollectionGenerator_Weapons.weapons.Clear();
			ItemCollectionGenerator_Weapons.weapons.AddRange(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsWeapon && !x.IsIngestible && x != ThingDefOf.WoodLog && x != ThingDefOf.ElephantTusk
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			TechLevel techLevel = parms.techLevel;
			if (techLevel >= TechLevel.Spacer)
			{
				return from x in ItemCollectionGenerator_Weapons.weapons
				where x.techLevel >= TechLevel.Industrial && x.techLevel <= techLevel
				select x;
			}
			return from x in ItemCollectionGenerator_Weapons.weapons
			where x.techLevel <= techLevel
			select x;
		}
	}
}
