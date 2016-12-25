using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasDutyPawnTarget : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.duty != null && pawn.mindState.duty.focus.Thing is Pawn;
		}
	}
}
