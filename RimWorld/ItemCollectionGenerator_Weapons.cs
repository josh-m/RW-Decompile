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
			where x.IsWeapon && !x.IsIngestible && x != ThingDefOf.WoodLog && x != ThingDefOf.ElephantTusk && (x.itemGeneratorTags == null || !x.itemGeneratorTags.Contains(ItemCollectionGeneratorUtility.SpecialRewardTag))
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			ItemCollectionGenerator_Weapons.<AllowedDefs>c__AnonStorey0 <AllowedDefs>c__AnonStorey = new ItemCollectionGenerator_Weapons.<AllowedDefs>c__AnonStorey0();
			ItemCollectionGenerator_Weapons.<AllowedDefs>c__AnonStorey0 arg_28_0 = <AllowedDefs>c__AnonStorey;
			TechLevel? techLevel = parms.techLevel;
			arg_28_0.techLevel = ((!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value);
			if (<AllowedDefs>c__AnonStorey.techLevel >= TechLevel.Spacer)
			{
				return from x in ItemCollectionGenerator_Weapons.weapons
				where x.techLevel >= TechLevel.Industrial && x.techLevel <= <AllowedDefs>c__AnonStorey.techLevel
				select x;
			}
			return from x in ItemCollectionGenerator_Weapons.weapons
			where x.techLevel <= <AllowedDefs>c__AnonStorey.techLevel
			select x;
		}
	}
}
