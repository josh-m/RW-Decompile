using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalStarving : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.needs.food != null && pawn.needs.food.CurCategory >= HungerCategory.Starving;
		}
	}
}
