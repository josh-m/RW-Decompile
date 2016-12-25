using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_UnloadYourInventory : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.inventory.UnloadEverything || pawn.inventory.innerContainer.Count == 0)
			{
				return null;
			}
			return new Job(JobDefOf.UnloadYourInventory);
		}
	}
}
