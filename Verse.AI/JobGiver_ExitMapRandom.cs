using RimWorld;
using System;

namespace Verse.AI
{
	public class JobGiver_ExitMapRandom : JobGiver_ExitMap
	{
		protected override bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 spot)
		{
			TraverseMode mode = (!canDig) ? TraverseMode.ByPawn : TraverseMode.PassAllDestroyableThings;
			return RCellFinder.TryFindRandomExitSpot(pawn, out spot, mode);
		}
	}
}
