using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_KeepLyingDown : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.CurJob != null && pawn.jobs.curDriver.layingDown)
			{
				return pawn.CurJob;
			}
			return null;
		}
	}
}
