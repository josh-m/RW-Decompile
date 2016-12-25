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

		public LordToil_PrepareCaravan_Leave(IntVec3 exitSpot)
		{
			this.exitSpot = exitSpot;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				if (pawn.IsColonist)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.Travel, this.exitSpot, -1f);
				}
				else
				{
					Pawn pawn2 = this.FindColonistToFollow(pawn);
					if (pawn2 != null)
					{
						pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, this.FindColonistToFollow(pawn), -1f);
					}
					else
					{
						pawn.mindState.duty = new PawnDuty(DutyDefOf.Travel, this.exitSpot, -1f);
					}
				}
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

		private Pawn FindColonistToFollow(Pawn forPawn)
		{
			float num = 0f;
			Pawn pawn = null;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn2 = this.lord.ownedPawns[i];
				if (pawn2 != forPawn)
				{
					if (pawn2.IsColonist)
					{
						float num2 = pawn2.Position.DistanceToSquared(forPawn.Position);
						if (pawn == null || num2 < num)
						{
							pawn = pawn2;
							num = num2;
						}
					}
				}
			}
			return pawn;
		}
	}
}
