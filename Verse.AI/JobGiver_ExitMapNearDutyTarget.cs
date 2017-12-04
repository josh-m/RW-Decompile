using RimWorld;
using System;

namespace Verse.AI
{
	public class JobGiver_ExitMapNearDutyTarget : JobGiver_ExitMap
	{
		protected override bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 spot)
		{
			TraverseMode mode = (!canDig) ? TraverseMode.ByPawn : TraverseMode.PassAllDestroyableThings;
			IntVec3 near = pawn.DutyLocation();
			float num = pawn.mindState.duty.radius;
			if (num <= 0f)
			{
				num = 12f;
			}
			return RCellFinder.TryFindExitSpotNear(pawn, near, num, out spot, mode);
		}
	}
}
