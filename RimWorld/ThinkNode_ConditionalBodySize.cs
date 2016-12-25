using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalBodySize : ThinkNode_Conditional
	{
		public float min;

		public float max = 99999f;

		protected override bool Satisfied(Pawn pawn)
		{
			float bodySize = pawn.BodySize;
			return bodySize >= this.min && bodySize <= this.max;
		}
	}
}
