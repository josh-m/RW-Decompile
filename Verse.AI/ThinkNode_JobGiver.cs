using System;

namespace Verse.AI
{
	public abstract class ThinkNode_JobGiver : ThinkNode
	{
		protected abstract Job TryGiveJob(Pawn pawn);

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			ThinkResult result;
			try
			{
				if (jobParams.maxDistToSquadFlag > 0f)
				{
					if (pawn.mindState.maxDistToSquadFlag > 0f)
					{
						Log.Error("Squad flag was not reset properly; raiders may behave strangely");
					}
					pawn.mindState.maxDistToSquadFlag = jobParams.maxDistToSquadFlag;
				}
				Job job = this.TryGiveJob(pawn);
				if (job == null)
				{
					result = ThinkResult.NoJob;
				}
				else
				{
					result = new ThinkResult(job, this, null);
				}
			}
			finally
			{
				pawn.mindState.maxDistToSquadFlag = -1f;
			}
			return result;
		}
	}
}
