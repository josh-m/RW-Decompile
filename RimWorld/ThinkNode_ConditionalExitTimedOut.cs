using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalExitTimedOut : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.exitMapAfterTick >= 0 && Find.TickManager.TicksGame > pawn.mindState.exitMapAfterTick;
		}
	}
}
