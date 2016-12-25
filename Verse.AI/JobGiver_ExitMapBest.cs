using RimWorld;
using System;

namespace Verse.AI
{
	public class JobGiver_ExitMapBest : JobGiver_ExitMap
	{
		protected override bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 spot)
		{
			TraverseMode mode = canDig ? TraverseMode.PassAnything : TraverseMode.ByPawn;
			return RCellFinder.TryFindBestExitSpot(pawn, out spot, mode);
		}
	}
}
