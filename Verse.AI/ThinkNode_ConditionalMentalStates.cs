using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalStates : ThinkNode_Priority
	{
		private List<MentalStateDef> states;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalStates thinkNode_ConditionalMentalStates = (ThinkNode_ConditionalMentalStates)base.DeepCopy(resolve);
			thinkNode_ConditionalMentalStates.states = this.states.ListFullCopyOrNull<MentalStateDef>();
			return thinkNode_ConditionalMentalStates;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (!this.states.Contains(pawn.MentalStateDef))
			{
				return ThinkResult.NoJob;
			}
			return base.TryIssueJobPackage(pawn);
		}
	}
}
