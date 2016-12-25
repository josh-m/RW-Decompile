using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalNeedPercentageAbove : ThinkNode_Conditional
	{
		private NeedDef need;

		private float threshold;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalNeedPercentageAbove thinkNode_ConditionalNeedPercentageAbove = (ThinkNode_ConditionalNeedPercentageAbove)base.DeepCopy(resolve);
			thinkNode_ConditionalNeedPercentageAbove.need = this.need;
			thinkNode_ConditionalNeedPercentageAbove.threshold = this.threshold;
			return thinkNode_ConditionalNeedPercentageAbove;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.needs.TryGetNeed(this.need).CurLevelPercentage > this.threshold;
		}
	}
}
