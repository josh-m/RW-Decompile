using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SeekAllowedArea : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.Position.IsForbidden(pawn))
			{
				return null;
			}
			if (this.HasJobWithSpawnedAllowedTarget(pawn))
			{
				return null;
			}
			Region region = pawn.GetRegion(RegionType.Set_Passable);
			if (region == null)
			{
				return null;
			}
			TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
			Region reg = null;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.portal != null)
				{
					return false;
				}
				if (!r.IsForbiddenEntirely(pawn))
				{
					reg = r;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, RegionType.Set_Passable);
			if (reg == null)
			{
				return null;
			}
			IntVec3 c;
			if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, null, out c))
			{
				return null;
			}
			return new Job(JobDefOf.Goto, c);
		}

		private bool HasJobWithSpawnedAllowedTarget(Pawn pawn)
		{
			Job curJob = pawn.CurJob;
			return curJob != null && (this.IsSpawnedAllowedTarget(curJob.targetA, pawn) || this.IsSpawnedAllowedTarget(curJob.targetB, pawn) || this.IsSpawnedAllowedTarget(curJob.targetC, pawn));
		}

		private bool IsSpawnedAllowedTarget(LocalTargetInfo target, Pawn pawn)
		{
			if (!target.IsValid)
			{
				return false;
			}
			if (target.HasThing)
			{
				return target.Thing.Spawned && !target.Thing.Position.IsForbidden(pawn);
			}
			return target.Cell.InBounds(pawn.Map) && !target.Cell.IsForbidden(pawn);
		}
	}
}
