using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Artifacts : ItemCollectionGenerator_Standard
	{
		public static List<ThingDef> artifacts = new List<ThingDef>();

		public static void Reset()
		{
			ItemCollectionGenerator_Artifacts.artifacts.Clear();
			ItemCollectionGenerator_Artifacts.artifacts.AddRange(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.HasComp(typeof(CompUseEffect_Artifact))
			select x);
		}

		protected override IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			return from x in ItemCollectionGenerator_Artifacts.artifacts
			where x.techLevel <= parms.techLevel
			select x;
		}
	}
}
