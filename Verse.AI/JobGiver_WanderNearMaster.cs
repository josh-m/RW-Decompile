using System;

namespace Verse.AI
{
	public class JobGiver_WanderNearMaster : JobGiver_Wander
	{
		public JobGiver_WanderNearMaster()
		{
			this.wanderRadius = 3f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return WanderUtility.BestCloseWanderRoot(pawn.playerSettings.master.PositionHeld, pawn);
		}
	}
}
