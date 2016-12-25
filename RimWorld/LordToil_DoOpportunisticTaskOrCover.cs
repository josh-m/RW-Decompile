using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_DoOpportunisticTaskOrCover : LordToil
	{
		public bool cover = true;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		protected abstract DutyDef DutyDef
		{
			get;
		}

		protected abstract bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets);

		public override void UpdateAllDuties()
		{
			List<Thing> list = null;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				Thing item = null;
				if (!this.cover || (this.TryFindGoodOpportunisticTaskTarget(pawn, out item, list) && !GenAI.InDangerousCombat(pawn)))
				{
					pawn.mindState.duty = new PawnDuty(this.DutyDef);
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
					if (list == null)
					{
						list = new List<Thing>();
					}
					list.Add(item);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				}
			}
		}

		public override void LordToilTick()
		{
			if (this.cover && Find.TickManager.TicksGame % 181 == 0)
			{
				List<Thing> list = null;
				for (int i = 0; i < this.lord.ownedPawns.Count; i++)
				{
					Pawn pawn = this.lord.ownedPawns[i];
					if (!pawn.Downed && pawn.mindState.duty.def == DutyDefOf.AssaultColony)
					{
						Thing thing = null;
						if (this.TryFindGoodOpportunisticTaskTarget(pawn, out thing, list) && !base.Map.reservationManager.IsReserved(thing, this.lord.faction) && !GenAI.InDangerousCombat(pawn))
						{
							pawn.mindState.duty = new PawnDuty(this.DutyDef);
							pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
							if (list == null)
							{
								list = new List<Thing>();
							}
							list.Add(thing);
						}
					}
				}
			}
		}
	}
}
