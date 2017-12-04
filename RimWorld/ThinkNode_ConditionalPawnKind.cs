using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalPawnKind : ThinkNode_Conditional
	{
		public PawnKindDef pawnKind;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalPawnKind thinkNode_ConditionalPawnKind = (ThinkNode_ConditionalPawnKind)base.DeepCopy(resolve);
			thinkNode_ConditionalPawnKind.pawnKind = this.pawnKind;
			return thinkNode_ConditionalPawnKind;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.kindDef == this.pawnKind;
		}
	}
}
