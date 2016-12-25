using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalReleased : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.guest != null && pawn.guest.released;
		}
	}
}
