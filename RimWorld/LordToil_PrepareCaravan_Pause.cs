using System;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_Pause : LordToil
	{
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Pause);
			}
		}
	}
}
