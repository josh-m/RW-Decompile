using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalCanDoConstantThinkTreeJobNow : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.Downed && !pawn.IsBurning() && !pawn.InMentalState && !pawn.Drafted && pawn.Awake();
		}
	}
}
