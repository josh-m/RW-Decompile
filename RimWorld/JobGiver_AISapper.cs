using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_AISapper : ThinkNode_JobGiver
	{
		private const float ReachDestDist = 10f;

		private const int CheckOverrideInterval = 500;

		private bool canMineNonMineables = true;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AISapper jobGiver_AISapper = (JobGiver_AISapper)base.DeepCopy(resolve);
			jobGiver_AISapper.canMineNonMineables = this.canMineNonMineables;
			return jobGiver_AISapper;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 intVec = (IntVec3)pawn.mindState.duty.focus;
			if (intVec.IsValid && intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom() && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors))
			{
				pawn.GetLord().Notify_ReachedDutyLocation(pawn);
				return null;
			}
			if (!intVec.IsValid)
			{
				IAttackTarget attackTarget;
				if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
				where !x.ThreatDisabled() && ((Thing)x).Faction == Faction.OfPlayer
				select x).TryRandomElement(out attackTarget))
				{
					return null;
				}
				intVec = ((Thing)attackTarget).Position;
			}
			using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything, false), PathEndMode.OnCell))
			{
				IntVec3 cellBeforeBlocker;
				Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
				if (thing != null)
				{
					Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, this.canMineNonMineables);
					if (job != null)
					{
						return job;
					}
				}
			}
			return new Job(JobDefOf.Goto, intVec, 500, true);
		}
	}
}
