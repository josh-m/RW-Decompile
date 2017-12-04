using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FleeAndCower : JobDriver_Flee
	{
		private const int CowerTicks = 1200;

		private const int CheckFleeAgainIntervalTicks = 35;

		public override string GetReport()
		{
			if (this.pawn.CurJob != this.job || this.pawn.Position != this.job.GetTarget(TargetIndex.A).Cell)
			{
				return base.GetReport();
			}
			return "ReportCowering".Translate();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
			}
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 1200,
				tickAction = delegate
				{
					if (this.$this.pawn.IsHashIntervalTick(35) && SelfDefenseUtility.ShouldStartFleeing(this.$this.pawn))
					{
						this.$this.EndJobWith(JobCondition.InterruptForced);
					}
				}
			};
		}
	}
}
