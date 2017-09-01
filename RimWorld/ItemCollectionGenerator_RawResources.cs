using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_RawResources : ItemCollectionGenerator_Standard
	{
		private static List<ThingDef> rawResources = new List<ThingDef>();

		public static void Reset()
		{
			ItemCollectionGenerator_RawResources.rawResources.Clear();
			ItemCollectionGenerator_RawResources.rawResources.AddRange(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsWithinCategory(ThingCategoryDefOf.ResourcesRaw)
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			return from x in ItemCollectionGenerator_RawResources.rawResources
			where x.techLevel <= parms.techLevel
			select x;
		}
	}
}
