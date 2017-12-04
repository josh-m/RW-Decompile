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
			ItemCollectionGenerator_RawResources.<AllowedDefs>c__AnonStorey0 <AllowedDefs>c__AnonStorey = new ItemCollectionGenerator_RawResources.<AllowedDefs>c__AnonStorey0();
			ItemCollectionGenerator_RawResources.<AllowedDefs>c__AnonStorey0 arg_28_0 = <AllowedDefs>c__AnonStorey;
			TechLevel? techLevel = parms.techLevel;
			arg_28_0.techLevel = ((!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value);
			return from x in ItemCollectionGenerator_RawResources.rawResources
			where x.techLevel <= <AllowedDefs>c__AnonStorey.techLevel
			select x;
		}
	}
}
