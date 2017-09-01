using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	internal class Recipe_ShutDown : RecipeWorker
	{
		[DebuggerHidden]
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (brain != null)
			{
				yield return brain;
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			pawn.health.AddHediff(this.recipe.addsHediff, part, null);
			ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.GenericHumane);
		}
	}
}
