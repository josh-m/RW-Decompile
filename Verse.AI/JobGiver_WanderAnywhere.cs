using System;

namespace Verse.AI
{
	public class JobGiver_WanderAnywhere : JobGiver_Wander
	{
		public JobGiver_WanderAnywhere()
		{
			this.wanderRadius = 7f;
			this.locomotionUrgency = LocomotionUrgency.Walk;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
