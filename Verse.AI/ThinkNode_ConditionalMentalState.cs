using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalState : ThinkNode_Priority
	{
		private MentalStateDef state;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalState thinkNode_ConditionalMentalState = (ThinkNode_ConditionalMentalState)base.DeepCopy(resolve);
			thinkNode_ConditionalMentalState.state = this.state;
			return thinkNode_ConditionalMentalState;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (pawn.MentalStateDef != this.state)
			{
				return ThinkResult.NoJob;
			}
			return base.TryIssueJobPackage(pawn);
		}
	}
}
