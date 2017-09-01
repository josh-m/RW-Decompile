using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Party : LordToilData
	{
		public int ticksToNextPulse;

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksToNextPulse, "ticksToNextPulse", 0, false);
		}
	}
}
