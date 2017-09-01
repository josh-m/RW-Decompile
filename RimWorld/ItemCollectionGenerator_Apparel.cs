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
			where x.IsApparel
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			return from x in ItemCollectionGenerator_Apparel.apparel
			where x.techLevel <= parms.techLevel
			select x;
		}
	}
}
