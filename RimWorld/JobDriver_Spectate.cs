using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Spectate : JobDriver
	{
		private const TargetIndex MySpotOrChairInd = TargetIndex.A;

		private const TargetIndex WatchTargetInd = TargetIndex.B;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			bool haveChair = base.CurJob.GetTarget(TargetIndex.A).HasThing;
			if (haveChair)
			{
				this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				tickAction = delegate
				{
					this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.CurJob.GetTarget(TargetIndex.B).Cell);
					this.<>f__this.pawn.GainComfortFromCellIfPossible();
					if (this.<>f__this.pawn.IsHashIntervalTick(100))
					{
						this.<>f__this.pawn.jobs.CheckForJobOverride();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never,
				handlingFacing = true
			};
		}
	}
}
