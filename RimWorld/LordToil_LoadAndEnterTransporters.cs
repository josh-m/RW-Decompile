using System;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_LoadAndEnterTransporters : LordToil
	{
		private int transportersGroup = -1;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_LoadAndEnterTransporters(int transportersGroup)
		{
			this.transportersGroup = transportersGroup;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.LoadAndEnterTransporters);
				pawnDuty.transportersGroup = this.transportersGroup;
				this.lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}
	}
}
