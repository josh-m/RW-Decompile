using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class ThinkNode_Conditional : ThinkNode_Priority
	{
		public bool invert;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_Conditional thinkNode_Conditional = (ThinkNode_Conditional)base.DeepCopy(resolve);
			thinkNode_Conditional.invert = this.invert;
			return thinkNode_Conditional;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn)
		{
			if (this.Satisfied(pawn) == !this.invert)
			{
				return base.TryIssueJobPackage(pawn);
			}
			return ThinkResult.NoJob;
		}

		protected abstract bool Satisfied(Pawn pawn);
	}
}
