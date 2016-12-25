using RimWorld;
using System;

namespace Verse.AI
{
	public class JobGiver_WanderMapEdge : JobGiver_Wander
	{
		public JobGiver_WanderMapEdge()
		{
			this.wanderRadius = 7f;
			this.ticksBetweenWandersRange = new IntRange(50, 125);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			IntVec3 result;
			if (RCellFinder.TryFindBestExitSpot(pawn, out result, TraverseMode.ByPawn))
			{
				return result;
			}
			return pawn.Position;
		}
	}
}
