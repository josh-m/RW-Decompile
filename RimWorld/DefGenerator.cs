using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DefGenerator
	{
		public static void GenerateImpliedDefs_PreResolve()
		{
			IEnumerable<ThingDef> enumerable = ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Meat.ImpliedMeatDefs()).Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs()).Concat(ThingDefGenerator_Leather.ImpliedLeatherDefs());
			foreach (ThingDef current in enumerable)
			{
				current.PostLoad();
				DefDatabase<ThingDef>.Add(current);
			}
			CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			foreach (TerrainDef current2 in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
			{
				current2.PostLoad();
				DefDatabase<TerrainDef>.Add(current2);
			}
			foreach (RecipeDef current3 in RecipeDefGenerator.ImpliedRecipeDefs())
			{
				current3.PostLoad();
				DefDatabase<RecipeDef>.Add(current3);
			}
		}

		public static void GenerateImpliedDefs_PostResolve()
		{
			foreach (KeyBindingCategoryDef current in KeyBindingDefGenerator.ImpliedKeyBindingCategoryDefs())
			{
				current.PostLoad();
				DefDatabase<KeyBindingCategoryDef>.Add(current);
			}
			foreach (KeyBindingDef current2 in KeyBindingDefGenerator.ImpliedKeyBindingDefs())
			{
				current2.PostLoad();
				DefDatabase<KeyBindingDef>.Add(current2);
			}
		}
	}
}
