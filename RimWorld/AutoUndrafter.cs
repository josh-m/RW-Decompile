using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class AutoUndrafter : IExposable
	{
		private const int UndraftDelay = 5000;

		private Pawn pawn;

		private int lastNonWaitingTick;

		public AutoUndrafter(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.lastNonWaitingTick, "lastNonWaitingTick", 0, false);
		}

		public void AutoUndraftTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				if (!this.pawn.drafter.Drafted)
				{
					this.lastNonWaitingTick = Find.TickManager.TicksGame;
				}
				else
				{
					if (this.pawn.jobs.curJob != null && this.pawn.jobs.curJob.def != JobDefOf.WaitCombat)
					{
						this.lastNonWaitingTick = Find.TickManager.TicksGame;
					}
					if (this.ShouldAutoUndraft())
					{
						this.pawn.drafter.Drafted = false;
					}
				}
			}
		}

		private bool ShouldAutoUndraft()
		{
			if (Find.TickManager.TicksGame - this.lastNonWaitingTick < 5000)
			{
				return false;
			}
			return !this.pawn.Map.attackTargetsCache.GetPotentialTargetsFor(this.pawn).Any((IAttackTarget x) => !x.ThreatDisabled());
		}
	}
}
