using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_AdministerIngestible : Recipe_Surgery
	{
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			ingredients[0].Ingested(pawn, 0f);
		}

		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			return pawn.Faction != billDoerFaction && this.recipe.ingredients[0].filter.AllowedThingDefs.First<ThingDef>().ingestible.drugCategory != DrugCategory.Medical;
		}
	}
}
