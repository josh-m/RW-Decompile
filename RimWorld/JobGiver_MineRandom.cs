using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MineRandom : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Region region = pawn.GetRegion();
			if (region == null)
			{
				return null;
			}
			for (int i = 0; i < 40; i++)
			{
				IntVec3 randomCell = region.RandomCell;
				for (int j = 0; j < 4; j++)
				{
					IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
					Building edifice = c.GetEdifice(pawn.Map);
					if (edifice != null && (edifice.def.passability == Traversability.Impassable || edifice.def.IsDoor) && edifice.def.size == IntVec2.One && edifice.def != ThingDefOf.CollapsedRocks && pawn.CanReserve(edifice, 1))
					{
						return new Job(JobDefOf.Mine, edifice)
						{
							ignoreDesignations = true
						};
					}
				}
			}
			return null;
		}
	}
}
