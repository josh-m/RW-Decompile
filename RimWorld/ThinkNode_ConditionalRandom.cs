using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalRandom : ThinkNode_Conditional
	{
		public float chance = 0.5f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalRandom thinkNode_ConditionalRandom = (ThinkNode_ConditionalRandom)base.DeepCopy(resolve);
			thinkNode_ConditionalRandom.chance = this.chance;
			return thinkNode_ConditionalRandom;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return Rand.Value < this.chance;
		}
	}
}
