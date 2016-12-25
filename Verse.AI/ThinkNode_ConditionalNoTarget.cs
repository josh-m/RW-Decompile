using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalNoTarget : ThinkNode_Priority
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (pawn.mindState.enemyTarget != null)
			{
				return ThinkResult.NoJob;
			}
			return base.TryIssueJobPackage(pawn);
		}
	}
}
