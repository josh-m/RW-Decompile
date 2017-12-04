using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class RecipeWorker
	{
		public RecipeDef recipe;

		[DebuggerHidden]
		public virtual IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
		}

		public virtual void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
		}

		public virtual bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			return pawn.Faction != billDoerFaction && this.recipe.isViolation;
		}

		public virtual string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			return this.recipe.LabelCap;
		}

		public virtual void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
			ingredient.Destroy(DestroyMode.Vanish);
		}
	}
}
