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
			Region validRegionAt = Find.RegionGrid.GetValidRegionAt(pawn.Position);
			if (validRegionAt == null)
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
			RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, regionProcessor, 9999);
			if (reg == null)
			{
				return null;
			}
			IntVec3 vec;
			if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, null, out vec))
			{
				return null;
			}
			return new Job(JobDefOf.Goto, vec);
		}
	}
}
