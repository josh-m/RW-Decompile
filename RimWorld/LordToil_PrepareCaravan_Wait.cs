using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_Wait : LordToil
	{
		private IntVec3 meetingPoint;

		public override float? CustomWakeThreshold
		{
			get
			{
				return new float?(0.5f);
			}
		}

		public override bool AllowRestingInBed
		{
			get
			{
				return false;
			}
		}

		public LordToil_PrepareCaravan_Wait(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, this.meetingPoint, -1f);
			}
		}
	}
}
