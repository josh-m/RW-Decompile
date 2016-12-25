using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyAutoJoinableCaravan : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return CaravanExitMapUtility.FindCaravanToJoinFor(pawn) != null;
		}
	}
}
