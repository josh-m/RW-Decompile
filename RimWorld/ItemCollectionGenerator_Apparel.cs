using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Apparel : ItemCollectionGenerator_Standard
	{
		private static List<ThingDef> apparel = new List<ThingDef>();

		public static void Reset()
		{
			ItemCollectionGenerator_Apparel.apparel.Clear();
			ItemCollectionGenerator_Apparel.apparel.AddRange(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsApparel && (x.itemGeneratorTags == null || !x.itemGeneratorTags.Contains(ItemCollectionGeneratorUtility.SpecialRewardTag))
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			ItemCollectionGenerator_Apparel.<AllowedDefs>c__AnonStorey0 <AllowedDefs>c__AnonStorey = new ItemCollectionGenerator_Apparel.<AllowedDefs>c__AnonStorey0();
			ItemCollectionGenerator_Apparel.<AllowedDefs>c__AnonStorey0 arg_28_0 = <AllowedDefs>c__AnonStorey;
			TechLevel? techLevel = parms.techLevel;
			arg_28_0.techLevel = ((!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value);
			return from x in ItemCollectionGenerator_Apparel.apparel
			where x.techLevel <= <AllowedDefs>c__AnonStorey.techLevel
			select x;
		}
	}
}
