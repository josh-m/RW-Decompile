using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalBleeding : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.health.hediffSet.BleedingRate > 0.001f;
		}
	}
}
