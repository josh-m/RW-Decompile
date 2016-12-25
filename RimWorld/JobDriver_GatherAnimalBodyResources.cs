using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_GatherAnimalBodyResources : JobDriver
	{
		protected const TargetIndex AnimalInd = TargetIndex.A;

		protected abstract int Duration
		{
			get;
		}

		protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = new Toil();
			prepare.initAction = delegate
			{
				Pawn pawn = this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
				if (pawn != null)
				{
					PawnUtility.ForceWait(pawn, this.<>f__this.Duration, null);
				}
			};
			prepare.defaultCompleteMode = ToilCompleteMode.Delay;
			prepare.defaultDuration = this.Duration;
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.GetComp((Pawn)((Thing)this.<>f__this.CurJob.GetTarget(TargetIndex.A))).Gathered(this.<>f__this.pawn);
				}
			};
		}
	}
}
