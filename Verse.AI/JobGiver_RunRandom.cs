using System;

namespace Verse.AI
{
	public class JobGiver_RunRandom : JobGiver_Wander
	{
		public JobGiver_RunRandom()
		{
			this.wanderRadius = 7f;
			this.ticksBetweenWandersRange = new IntRange(5, 10);
			this.locomotionUrgency = LocomotionUrgency.Sprint;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
