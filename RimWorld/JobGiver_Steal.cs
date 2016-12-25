using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Steal : ThinkNode_JobGiver
	{
		public const float ItemsSearchRadiusInitial = 7f;

		private const float ItemsSearchRadiusOngoing = 12f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 vec;
			if (!RCellFinder.TryFindBestExitSpot(pawn, out vec, TraverseMode.ByPawn))
			{
				return null;
			}
			Thing thing;
			if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, 12f, out thing, pawn, null) && !GenAI.InDangerousCombat(pawn))
			{
				return new Job(JobDefOf.Steal)
				{
					targetA = thing,
					targetB = vec,
					maxNumToCarry = Mathf.Min(thing.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / thing.def.VolumePerUnit))
				};
			}
			return null;
		}
	}
}
