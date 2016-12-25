using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalCannotReachMapEdge : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.CanReachMapEdge();
		}
	}
}
