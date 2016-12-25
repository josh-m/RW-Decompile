using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalColonyFaction : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Faction == Faction.OfPlayer;
		}
	}
}
