using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_JumpInWater : ThinkNode_JobGiver
	{
		private const float ActivateChance = 1f;

		private readonly IntRange MaxDistance = new IntRange(10, 16);

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 1f)
			{
				IntVec3 position = pawn.Position;
				Predicate<IntVec3> validator = (IntVec3 pos) => pos.GetTerrain(pawn.Map).extinguishesFire;
				Map map = pawn.Map;
				int randomInRange = this.MaxDistance.RandomInRange;
				IntVec3 c;
				if (RCellFinder.TryFindRandomCellNearWith(position, validator, map, out c, 5, randomInRange))
				{
					return new Job(JobDefOf.Goto, c);
				}
			}
			return null;
		}
	}
}
