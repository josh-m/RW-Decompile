using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalHasFallbackLocation : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.duty != null && pawn.mindState.duty.focusSecond.IsValid;
		}
	}
}
