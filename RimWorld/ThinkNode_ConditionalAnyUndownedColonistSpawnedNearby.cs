using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyUndownedColonistSpawnedNearby : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			bool arg_40_0;
			if (pawn.Spawned)
			{
				arg_40_0 = pawn.Map.mapPawns.FreeColonistsSpawned.Any((Pawn x) => !x.Downed);
			}
			else
			{
				arg_40_0 = false;
			}
			return arg_40_0;
		}
	}
}
