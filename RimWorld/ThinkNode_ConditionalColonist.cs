using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalColonist : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.IsColonist;
		}
	}
}
