using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	internal class Recipe_ExecuteByCut : RecipeWorker
	{
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			ExecutionUtility.DoExecutionByCut(billDoer, pawn);
			ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.GenericHumane);
		}
	}
}
