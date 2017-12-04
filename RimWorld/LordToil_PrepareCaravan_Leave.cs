using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_Leave : LordToil
	{
		private IntVec3 exitSpot;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

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

		public LordToil_PrepareCaravan_Leave(IntVec3 exitSpot)
		{
			this.exitSpot = exitSpot;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				pawn.mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait, this.exitSpot, -1f);
				pawn.mindState.duty.locomotion = LocomotionUrgency.Jog;
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				GatherAnimalsAndSlavesForCaravanUtility.CheckArrived(this.lord, this.exitSpot, "ReadyToExitMap", (Pawn x) => true, null);
			}
		}
	}
}
