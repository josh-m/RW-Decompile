using System;

namespace Verse.AI
{
	public static class JobQueueUtility
	{
		public static bool NextJobIsPlayerForced(Pawn pawn)
		{
			return pawn.jobQueue != null && pawn.jobQueue.Count != 0 && pawn.jobQueue.Peek().playerForced;
		}
	}
}
