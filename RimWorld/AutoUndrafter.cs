using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class AutoUndrafter : IExposable
	{
		private Pawn pawn;

		private int lastNonWaitingTick;

		private const int UndraftDelay = 5000;

		public AutoUndrafter(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.lastNonWaitingTick, "lastNonWaitingTick", 0, false);
		}

		public void AutoUndraftTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0 && this.pawn.Drafted)
			{
				if ((this.pawn.jobs.curJob != null && this.pawn.jobs.curJob.def != JobDefOf.WaitCombat) || this.AnyHostilePreventingAutoUndraft())
				{
					this.lastNonWaitingTick = Find.TickManager.TicksGame;
				}
				if (this.ShouldAutoUndraft())
				{
					this.pawn.drafter.Drafted = false;
				}
			}
		}

		public void Notify_Drafted()
		{
			this.lastNonWaitingTick = Find.TickManager.TicksGame;
		}

		private bool ShouldAutoUndraft()
		{
			return Find.TickManager.TicksGame - this.lastNonWaitingTick >= 5000 && !this.AnyHostilePreventingAutoUndraft();
		}

		private bool AnyHostilePreventingAutoUndraft()
		{
			List<IAttackTarget> potentialTargetsFor = this.pawn.Map.attackTargetsCache.GetPotentialTargetsFor(this.pawn);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				if (GenHostility.IsActiveThreatToPlayer(potentialTargetsFor[i]))
				{
					return true;
				}
			}
			return false;
		}
	}
}
