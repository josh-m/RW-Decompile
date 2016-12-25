using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PanicFlee : LordToil
	{
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public override void Init()
		{
			base.Init();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, false, false, null);
			}
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapRandom);
			}
		}
	}
}
