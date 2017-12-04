using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapNear : LordToil
	{
		private IntVec3 near;

		private float radius;

		private LocomotionUrgency locomotion;

		private bool canDig;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_ExitMapNear(IntVec3 near, float radius, LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false)
		{
			this.near = near;
			this.radius = radius;
			this.locomotion = locomotion;
			this.canDig = canDig;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.ExitMapNearDutyTarget, this.near, this.radius);
				pawnDuty.locomotion = this.locomotion;
				pawnDuty.canDig = this.canDig;
				this.lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}
	}
}
