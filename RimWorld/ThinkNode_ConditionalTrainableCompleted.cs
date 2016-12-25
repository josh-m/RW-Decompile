using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalTrainableCompleted : ThinkNode_Conditional
	{
		private TrainableDef trainable;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalTrainableCompleted thinkNode_ConditionalTrainableCompleted = (ThinkNode_ConditionalTrainableCompleted)base.DeepCopy(resolve);
			thinkNode_ConditionalTrainableCompleted.trainable = this.trainable;
			return thinkNode_ConditionalTrainableCompleted;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.training != null && pawn.training.IsCompleted(this.trainable);
		}
	}
}
