using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalDangerousTemperature : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.SafeTemperatureRange().Includes(pawn.Position.GetTemperature(pawn.Map));
		}
	}
}
