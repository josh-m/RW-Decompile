using System;

namespace Verse.AI
{
	public class JobGiver_ExitMapPanic : JobGiver_ExitMapBest
	{
		public JobGiver_ExitMapPanic()
		{
			this.canBash = true;
		}
	}
}
