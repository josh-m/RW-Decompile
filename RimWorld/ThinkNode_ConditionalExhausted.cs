using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalExhausted : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.needs.rest != null && pawn.needs.rest.CurCategory >= RestCategory.Exhausted;
		}
	}
}
