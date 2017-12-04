using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Maintain : JobDriver
	{
		private const int MaintainTicks = 180;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = Toils_General.Wait(180);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			Toil maintain = new Toil();
			maintain.initAction = delegate
			{
				Pawn actor = maintain.actor;
				CompMaintainable compMaintainable = actor.CurJob.targetA.Thing.TryGetComp<CompMaintainable>();
				compMaintainable.Maintained();
			};
			maintain.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return maintain;
		}
	}
}
