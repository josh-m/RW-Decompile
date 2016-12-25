using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GotoTravelDestination : ThinkNode_JobGiver
	{
		private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		private Danger maxDanger = Danger.Some;

		private int jobMaxDuration = 999999;

		private bool exactCell;

		private IntRange WaitTicks = new IntRange(30, 80);

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GotoTravelDestination jobGiver_GotoTravelDestination = (JobGiver_GotoTravelDestination)base.DeepCopy(resolve);
			jobGiver_GotoTravelDestination.locomotionUrgency = this.locomotionUrgency;
			jobGiver_GotoTravelDestination.maxDanger = this.maxDanger;
			jobGiver_GotoTravelDestination.jobMaxDuration = this.jobMaxDuration;
			jobGiver_GotoTravelDestination.exactCell = this.exactCell;
			return jobGiver_GotoTravelDestination;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			if (pawn.mindState.nextMoveOrderIsWait && !this.exactCell)
			{
				return new Job(JobDefOf.WaitWander)
				{
					expiryInterval = this.WaitTicks.RandomInRange
				};
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			if (!pawn.CanReach(cell, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger), false, TraverseMode.ByPawn))
			{
				return null;
			}
			if (this.exactCell && pawn.Position == cell)
			{
				return null;
			}
			IntVec3 c = cell;
			if (!this.exactCell)
			{
				c = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 6);
			}
			return new Job(JobDefOf.Goto, c)
			{
				locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.locomotionUrgency),
				expiryInterval = this.jobMaxDuration
			};
		}
	}
}
