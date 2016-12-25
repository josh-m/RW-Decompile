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

		public override ThinkNode DeepCopy()
		{
			JobGiver_ExitMap jobGiver_ExitMap = (JobGiver_ExitMap)base.DeepCopy();
			jobGiver_ExitMap.defaultLocomotion = this.defaultLocomotion;
			jobGiver_ExitMap.jobMaxDuration = this.jobMaxDuration;
			jobGiver_ExitMap.canBash = this.canBash;
			jobGiver_ExitMap.forceCanDig = this.forceCanDig;
			return jobGiver_ExitMap;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = false;
			if (this.forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig))
			{
				flag = true;
			}
			IntVec3 vec;
			if (!this.TryFindGoodExitDest(pawn, flag, out vec))
			{
				return null;
			}
			if (flag)
			{
				using (PawnPath pawnPath = PathFinder.FindPath(pawn.Position, vec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything, false), PathEndMode.OnCell))
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
			return new Job(JobDefOf.Goto, vec)
			{
				exitMapOnArrival = true,
				locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.defaultLocomotion, LocomotionUrgency.Jog),
				expiryInterval = this.jobMaxDuration,
				canBash = this.canBash
			};
		}

		protected abstract bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 dest);
	}
}
