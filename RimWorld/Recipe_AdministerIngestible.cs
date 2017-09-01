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
			if (pawn.IsTeetotaler() && ingredients[0].def.IsDrug)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ForcedMeToTakeDrugs, billDoer);
			}
			else if (pawn.IsProsthophobe() && ingredients[0].def == ThingDefOf.Luciferium)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ForcedMeToTakeLuciferium, billDoer);
			}
		}

		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			return pawn.Faction != billDoerFaction && this.recipe.ingredients[0].filter.AllowedThingDefs.First<ThingDef>().ingestible.drugCategory != DrugCategory.Medical;
		}

		public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.IsTeetotaler() && this.recipe.ingredients[0].filter.BestThingRequest.singleDef.IsDrug)
			{
				return base.GetLabelWhenUsedOn(pawn, part) + " (" + "TeetotalerUnhappy".Translate() + ")";
			}
			if (pawn.IsProsthophobe() && this.recipe.ingredients[0].filter.BestThingRequest.singleDef == ThingDefOf.Luciferium)
			{
				return base.GetLabelWhenUsedOn(pawn, part) + " (" + "ProsthophobeUnhappy".Translate() + ")";
			}
			return base.GetLabelWhenUsedOn(pawn, part);
		}
	}
}
