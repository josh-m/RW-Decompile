using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalState : ThinkNode_Conditional
	{
		public MentalStateDef state;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalState thinkNode_ConditionalMentalState = (ThinkNode_ConditionalMentalState)base.DeepCopy(resolve);
			thinkNode_ConditionalMentalState.state = this.state;
			return thinkNode_ConditionalMentalState;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.MentalStateDef == this.state;
		}
	}
}
