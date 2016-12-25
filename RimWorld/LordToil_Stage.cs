using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Stage : LordToil
	{
		public override IntVec3 FlagLoc
		{
			get
			{
				return this.Data.stagingPoint;
			}
		}

		private LordToilData_Stage Data
		{
			get
			{
				return (LordToilData_Stage)this.data;
			}
		}

		public LordToil_Stage(IntVec3 stagingLoc)
		{
			this.data = new LordToilData_Stage();
			this.Data.stagingPoint = stagingLoc;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Stage data = this.Data;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, data.stagingPoint, -1f);
				this.lord.ownedPawns[i].mindState.duty.radius = 28f;
			}
		}
	}
}
