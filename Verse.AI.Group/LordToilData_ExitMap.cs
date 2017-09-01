using System;

namespace Verse.AI.Group
{
	public class LordToilData_ExitMap : LordToilData
	{
		public LocomotionUrgency locomotion;

		public bool canDig;

		public override void ExposeData()
		{
			Scribe_Values.Look<LocomotionUrgency>(ref this.locomotion, "locomotion", LocomotionUrgency.None, false);
			Scribe_Values.Look<bool>(ref this.canDig, "canDig", false, false);
		}
	}
}
