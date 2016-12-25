using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Kidnap : ThinkNode_JobGiver
	{
		public const float VictimSearchRadiusInitial = 8f;

		private const float VictimSearchRadiusOngoing = 18f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 c;
			if (!RCellFinder.TryFindBestExitSpot(pawn, out c, TraverseMode.ByPawn))
			{
				return null;
			}
			Pawn t;
			if (KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 18f, out t, null) && !GenAI.InDangerousCombat(pawn))
			{
				return new Job(JobDefOf.Kidnap)
				{
					targetA = t,
					targetB = c,
					count = 1
				};
			}
			return null;
		}
	}
}
