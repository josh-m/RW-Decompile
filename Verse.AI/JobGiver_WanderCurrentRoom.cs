using System;

namespace Verse.AI
{
	public class JobGiver_WanderCurrentRoom : JobGiver_Wander
	{
		public JobGiver_WanderCurrentRoom()
		{
			this.wanderRadius = 7f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
			this.locomotionUrgency = LocomotionUrgency.Amble;
			this.wanderDestValidator = ((Pawn pawn, IntVec3 loc) => WanderRoomUtility.IsValidWanderDest(pawn, loc, this.GetWanderRoot(pawn)));
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
