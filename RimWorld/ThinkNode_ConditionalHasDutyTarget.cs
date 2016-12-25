using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasDutyTarget : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.duty != null && pawn.mindState.duty.focus.IsValid;
		}
	}
}
