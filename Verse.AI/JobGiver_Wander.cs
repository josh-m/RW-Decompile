using RimWorld;
using System;

namespace Verse.AI
{
	public abstract class JobGiver_Wander : ThinkNode_JobGiver
	{
		protected float wanderRadius;

		protected Func<Pawn, IntVec3, bool> wanderDestValidator;

		protected IntRange ticksBetweenWandersRange = new IntRange(20, 100);

		protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		protected Danger maxDanger = Danger.None;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Wander jobGiver_Wander = (JobGiver_Wander)base.DeepCopy(resolve);
			jobGiver_Wander.wanderRadius = this.wanderRadius;
			jobGiver_Wander.wanderDestValidator = this.wanderDestValidator;
			jobGiver_Wander.ticksBetweenWandersRange = this.ticksBetweenWandersRange;
			jobGiver_Wander.locomotionUrgency = this.locomotionUrgency;
			jobGiver_Wander.maxDanger = this.maxDanger;
			return jobGiver_Wander;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			if (pawn.mindState.nextMoveOrderIsWait)
			{
				return new Job(JobDefOf.WaitWander)
				{
					expiryInterval = this.ticksBetweenWandersRange.RandomInRange
				};
			}
			IntVec3 exactWanderDest = this.GetExactWanderDest(pawn);
			if (!exactWanderDest.IsValid)
			{
				return null;
			}
			pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, exactWanderDest);
			return new Job(JobDefOf.GotoWander, exactWanderDest)
			{
				locomotionUrgency = this.locomotionUrgency
			};
		}

		protected virtual IntVec3 GetExactWanderDest(Pawn pawn)
		{
			IntVec3 wanderRoot = this.GetWanderRoot(pawn);
			return RCellFinder.RandomWanderDestFor(pawn, wanderRoot, this.wanderRadius, this.wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger));
		}

		protected abstract IntVec3 GetWanderRoot(Pawn pawn);
	}
}
