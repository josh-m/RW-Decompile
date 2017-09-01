using System;
using UnityEngine;

namespace Verse.AI
{
	public class ThinkNode_ForbidOutsideFlagRadius : ThinkNode_Priority
	{
		public float maxDistToSquadFlag = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ForbidOutsideFlagRadius thinkNode_ForbidOutsideFlagRadius = (ThinkNode_ForbidOutsideFlagRadius)base.DeepCopy(resolve);
			thinkNode_ForbidOutsideFlagRadius.maxDistToSquadFlag = this.maxDistToSquadFlag;
			return thinkNode_ForbidOutsideFlagRadius;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (this.maxDistToSquadFlag > 0f)
			{
				if (jobParams.maxDistToSquadFlag > 0f)
				{
					jobParams.maxDistToSquadFlag = Mathf.Min(jobParams.maxDistToSquadFlag, this.maxDistToSquadFlag);
				}
				else
				{
					jobParams.maxDistToSquadFlag = this.maxDistToSquadFlag;
				}
			}
			return base.TryIssueJobPackage(pawn, jobParams);
		}
	}
}
