using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnimalWrongSeason : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.RaceProps.Animal && !GenTemperature.SeasonAcceptableFor(pawn.def);
		}
	}
}
