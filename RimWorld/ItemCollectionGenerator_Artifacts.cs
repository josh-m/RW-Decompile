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
			ItemCollectionGenerator_Artifacts.<AllowedDefs>c__AnonStorey0 <AllowedDefs>c__AnonStorey = new ItemCollectionGenerator_Artifacts.<AllowedDefs>c__AnonStorey0();
			ItemCollectionGenerator_Artifacts.<AllowedDefs>c__AnonStorey0 arg_28_0 = <AllowedDefs>c__AnonStorey;
			TechLevel? techLevel = parms.techLevel;
			arg_28_0.techLevel = ((!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value);
			return from x in ItemCollectionGenerator_Artifacts.artifacts
			where x.techLevel <= <AllowedDefs>c__AnonStorey.techLevel
			select x;
		}
	}
}
