using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnTechHediffsGenerator
	{
		private static List<Thing> emptyIngredientsList = new List<Thing>();

		public static void GeneratePartsAndImplantsFor(Pawn pawn)
		{
			if (pawn.kindDef.techHediffsTags == null)
			{
				return;
			}
			if (Rand.Value > pawn.kindDef.techHediffsChance)
			{
				return;
			}
			float partsMoney = pawn.kindDef.techHediffsMoney.RandomInRange;
			IEnumerable<ThingDef> source = from x in DefDatabase<ThingDef>.AllDefs
			where x.isBodyPartOrImplant && x.BaseMarketValue <= partsMoney && x.techHediffsTags != null && pawn.kindDef.techHediffsTags.Any((string tag) => x.techHediffsTags.Contains(tag))
			select x;
			if (source.Any<ThingDef>())
			{
				ThingDef partDef = source.RandomElementByWeight((ThingDef w) => w.BaseMarketValue);
				IEnumerable<RecipeDef> source2 = from x in DefDatabase<RecipeDef>.AllDefs
				where x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x)
				select x;
				if (source2.Any<RecipeDef>())
				{
					RecipeDef recipeDef = source2.RandomElement<RecipeDef>();
					if (recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).Any<BodyPartRecord>())
					{
						recipeDef.Worker.ApplyOnPawn(pawn, recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).RandomElement<BodyPartRecord>(), null, PawnTechHediffsGenerator.emptyIngredientsList);
					}
				}
			}
		}
	}
}
