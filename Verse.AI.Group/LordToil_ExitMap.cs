using System;

namespace Verse.AI.Group
{
	public abstract class LordToil_ExitMap : LordToil
	{
		protected abstract DutyDef DutyDef
		{
			get;
		}

		private LordToilData_ExitMap Data
		{
			get
			{
				return (LordToilData_ExitMap)this.data;
			}
		}

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_ExitMap(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false)
		{
			this.data = new LordToilData_ExitMap();
			this.Data.locomotion = locomotion;
			this.Data.canDig = canDig;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_ExitMap data = this.Data;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(this.DutyDef);
				pawnDuty.locomotion = data.locomotion;
				pawnDuty.canDig = data.canDig;
				this.lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}
	}
}
