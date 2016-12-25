using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrisonerEscape : LordToil_Travel
	{
		private int sapperThingID;

		public override IntVec3 FlagLoc
		{
			get
			{
				return this.Data.dest;
			}
		}

		private LordToilData_Travel Data
		{
			get
			{
				return (LordToilData_Travel)this.data;
			}
		}

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		protected override float AllArrivedCheckRadius
		{
			get
			{
				return 14f;
			}
		}

		public LordToil_PrisonerEscape(IntVec3 dest, int sapperThingID) : base(dest)
		{
			this.sapperThingID = sapperThingID;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Travel data = this.Data;
			Pawn leader = this.GetLeader();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				if (this.IsSapper(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscapeSapper, data.dest, -1f);
				}
				else if (leader == null || pawn == leader)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscape, data.dest, -1f);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscape, leader, 10f);
				}
			}
		}

		public override void LordToilTick()
		{
			base.LordToilTick();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				pawn.guilt.Notify_Guilty();
			}
		}

		private Pawn GetLeader()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				if (!this.lord.ownedPawns[i].Downed && this.IsSapper(this.lord.ownedPawns[i]))
				{
					return this.lord.ownedPawns[i];
				}
			}
			for (int j = 0; j < this.lord.ownedPawns.Count; j++)
			{
				if (!this.lord.ownedPawns[j].Downed)
				{
					return this.lord.ownedPawns[j];
				}
			}
			return null;
		}

		private bool IsSapper(Pawn p)
		{
			return p.thingIDNumber == this.sapperThingID;
		}
	}
}
