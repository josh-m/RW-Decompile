using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalInPartyArea : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return false;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			return PartyUtility.InPartyArea(pawn.Position, cell, pawn.Map);
		}
	}
}
