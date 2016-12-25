using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_GatherSlaves : LordToil
	{
		private IntVec3 meetingPoint;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_PrepareCaravan_GatherSlaves(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				if (!pawn.RaceProps.Animal)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherPawns, this.meetingPoint, -1f);
					pawn.mindState.duty.pawnsToGather = PawnsToGather.Slaves;
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, this.meetingPoint, -1f);
				}
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				GatherAnimalsAndSlavesForCaravanUtility.CheckArrived(this.lord, this.meetingPoint, "AllSlavesGathered", (Pawn x) => !x.IsColonist && !x.RaceProps.Animal, (Pawn x) => GatherAnimalsAndSlavesForCaravanUtility.IsFollowingAnyone(x));
			}
		}
	}
}
