using System;

namespace Verse.AI
{
	public class JobGiver_JobQueue : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.jobQueue == null || pawn.jobQueue.Count == 0)
			{
				return null;
			}
			return pawn.jobQueue.Dequeue();
		}
	}
}
