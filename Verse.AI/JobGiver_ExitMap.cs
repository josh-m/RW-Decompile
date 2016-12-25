using RimWorld;
using System;

namespace Verse.AI
{
	public abstract class JobGiver_ExitMap : ThinkNode_JobGiver
	{
		protected LocomotionUrgency defaultLocomotion;

		protected int jobMaxDuration = 999999;

		protected bool canBash;

		protected bool forceCanDig;

		protected bool failIfCantJoinOrCreateCaravan;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_ExitMap jobGiver_ExitMap = (JobGiver_ExitMap)base.DeepCopy(resolve);
			jobGiver_ExitMap.defaultLocomotion = this.defaultLocomotion;
			jobGiver_ExitMap.jobMaxDuration = this.jobMaxDuration;
			jobGiver_ExitMap.canBash = this.canBash;
			jobGiver_ExitMap.forceCanDig = this.forceCanDig;
			jobGiver_ExitMap.failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan;
			return jobGiver_ExitMap;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = false;
			if (this.forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig))
			{
				flag = true;
			}
			IntVec3 c;
			if (!this.TryFindGoodExitDest(pawn, flag, out c))
			{
				return null;
			}
			if (flag)
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything, false), PathEndMode.OnCell))
				{
					IntVec3 cellBeforeBlocker;
					Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
					if (thing != null)
					{
						Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
						if (job != null)
						{
							return job;
						}
					}
				}
			}
			return new Job(JobDefOf.Goto, c)
			{
				exitMapOnArrival = true,
				failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan,
				locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.defaultLocomotion, LocomotionUrgency.Jog),
				expiryInterval = this.jobMaxDuration,
				canBash = this.canBash
			};
		}

		protected abstract bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 dest);
	}
}
