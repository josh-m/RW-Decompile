using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class TriggerData_PawnCycleInd : TriggerData
	{
		public int pawnCycleInd;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.pawnCycleInd, "pawnCycleInd", 0, false);
		}
	}
}
