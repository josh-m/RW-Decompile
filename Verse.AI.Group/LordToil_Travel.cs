using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class LordToil_Travel : LordToil
	{
		public Danger maxDanger;

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

		protected virtual float AllArrivedCheckRadius
		{
			get
			{
				return 10f;
			}
		}

		public LordToil_Travel(IntVec3 dest)
		{
			this.data = new LordToilData_Travel();
			this.Data.dest = dest;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Travel data = this.Data;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.Travel, data.dest, -1f);
				pawnDuty.maxDanger = this.maxDanger;
				this.lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 205 == 0)
			{
				LordToilData_Travel data = this.Data;
				bool flag = true;
				for (int i = 0; i < this.lord.ownedPawns.Count; i++)
				{
					Pawn pawn = this.lord.ownedPawns[i];
					if (!pawn.Position.InHorDistOf(data.dest, this.AllArrivedCheckRadius) || !pawn.CanReach(data.dest, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					this.lord.ReceiveMemo("TravelArrived");
				}
			}
		}

		public bool HasDestination()
		{
			return this.Data.dest.IsValid;
		}

		public void SetDestination(IntVec3 dest)
		{
			this.Data.dest = dest;
		}
	}
}
