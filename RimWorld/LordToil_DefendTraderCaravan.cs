using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	internal class LordToil_DefendTraderCaravan : LordToil_DefendPoint
	{
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_DefendTraderCaravan() : base(true)
		{
		}

		public LordToil_DefendTraderCaravan(IntVec3 defendPoint) : base(defendPoint, 28f)
		{
		}

		public override void UpdateAllDuties()
		{
			LordToilData_DefendPoint data = base.Data;
			Pawn pawn = TraderCaravanUtility.FindTrader(this.lord);
			if (pawn != null)
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
				for (int i = 0; i < this.lord.ownedPawns.Count; i++)
				{
					Pawn pawn2 = this.lord.ownedPawns[i];
					switch (pawn2.GetTraderCaravanRole())
					{
					case TraderCaravanRole.Carrier:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Follow, pawn, 5f);
						pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
						break;
					case TraderCaravanRole.Guard:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
						break;
					case TraderCaravanRole.Chattel:
						pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, pawn, 5f);
						pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
						break;
					}
				}
				return;
			}
		}
	}
}
