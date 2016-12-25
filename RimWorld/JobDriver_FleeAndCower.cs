using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FleeAndCower : JobDriver
	{
		private const TargetIndex DestInd = TargetIndex.A;

		private const int CowerTicks = 1200;

		private const int CheckFleeAgainInvervalTicks = 35;

		public override string GetReport()
		{
			if (this.pawn.Position != base.CurJob.GetTarget(TargetIndex.A).Cell)
			{
				return base.GetReport();
			}
			return "ReportCowering".Translate();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					this.<>f__this.Map.pawnDestinationManager.ReserveDestinationFor(this.<>f__this.pawn, this.<>f__this.CurJob.GetTarget(TargetIndex.A).Cell);
					if (this.<>f__this.pawn.IsColonist)
					{
						MoteMaker.MakeColonistActionOverlay(this.<>f__this.pawn, ThingDefOf.Mote_ColonistFleeing);
					}
				}
			};
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 1200,
				tickAction = delegate
				{
					if (this.<>f__this.pawn.IsHashIntervalTick(35) && SelfDefenseUtility.ShouldStartFleeing(this.<>f__this.pawn))
					{
						this.<>f__this.EndJobWith(JobCondition.InterruptForced);
					}
				}
			};
		}
	}
}
