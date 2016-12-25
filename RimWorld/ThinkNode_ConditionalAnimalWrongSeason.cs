using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnimalWrongSeason : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.RaceProps.Animal && !pawn.Map.mapTemperature.SeasonAcceptableFor(pawn.def);
		}
	}
}
