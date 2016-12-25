using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_AssaultColonySappers : LordToilData
	{
		public IntVec3 sapperDest;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.sapperDest, "sapperDest", default(IntVec3), false);
		}
	}
}
