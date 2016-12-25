using System;

namespace Verse.AI.Group
{
	public class LordToilData_DefendPoint : LordToilData
	{
		public IntVec3 defendPoint = IntVec3.Invalid;

		public float defendRadius = 28f;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.defendPoint, "defendPoint", default(IntVec3), false);
			Scribe_Values.LookValue<float>(ref this.defendRadius, "defendRadius", 28f, false);
		}
	}
}
