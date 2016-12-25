using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalHasFallbackLocation : ThinkNode_Priority
	{
		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (pawn.mindState.duty == null || !pawn.mindState.duty.focusSecond.IsValid)
			{
				return ThinkResult.NoJob;
			}
			return base.TryIssueJobPackage(pawn);
		}
	}
}
