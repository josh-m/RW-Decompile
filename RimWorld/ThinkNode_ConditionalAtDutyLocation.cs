using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAtDutyLocation : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.duty != null && pawn.Position == pawn.mindState.duty.focus.Cell;
		}
	}
}
