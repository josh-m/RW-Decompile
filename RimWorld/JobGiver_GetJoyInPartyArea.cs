using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetJoyInPartyArea : JobGiver_GetJoy
	{
		protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return null;
			}
			if (pawn.needs.joy == null)
			{
				return null;
			}
			float curLevelPercentage = pawn.needs.joy.CurLevelPercentage;
			if (curLevelPercentage > 0.92f)
			{
				return null;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			return def.Worker.TryGiveJobInPartyArea(pawn, cell);
		}
	}
}
