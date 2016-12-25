using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalCloseToDutyTarget : ThinkNode_Conditional
	{
		private float maxDistToDutyTarget = 10f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalCloseToDutyTarget thinkNode_ConditionalCloseToDutyTarget = (ThinkNode_ConditionalCloseToDutyTarget)base.DeepCopy(resolve);
			thinkNode_ConditionalCloseToDutyTarget.maxDistToDutyTarget = this.maxDistToDutyTarget;
			return thinkNode_ConditionalCloseToDutyTarget;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.duty.focus.IsValid && pawn.Position.InHorDistOf(pawn.mindState.duty.focus.Cell, this.maxDistToDutyTarget);
		}
	}
}
