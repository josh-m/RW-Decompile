using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalPrisonerInPrisonCell : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (!pawn.IsPrisoner)
			{
				return false;
			}
			Room room = pawn.GetRoom(RegionType.Set_Passable);
			return room != null && room.isPrisonCell;
		}
	}
}
