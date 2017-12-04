using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalIntelligence : ThinkNode_Conditional
	{
		public Intelligence minIntelligence = Intelligence.ToolUser;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalIntelligence thinkNode_ConditionalIntelligence = (ThinkNode_ConditionalIntelligence)base.DeepCopy(resolve);
			thinkNode_ConditionalIntelligence.minIntelligence = this.minIntelligence;
			return thinkNode_ConditionalIntelligence;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.RaceProps.intelligence >= this.minIntelligence;
		}
	}
}
