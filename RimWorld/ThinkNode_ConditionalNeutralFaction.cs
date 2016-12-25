using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalNeutralFaction : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Faction != null && pawn.Faction != Faction.OfPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer);
		}
	}
}
