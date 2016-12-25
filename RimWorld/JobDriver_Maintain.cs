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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = new Toil();
			prepare.defaultCompleteMode = ToilCompleteMode.Delay;
			prepare.defaultDuration = 180;
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<maintain>__1.actor;
					CompMaintainable compMaintainable = actor.CurJob.targetA.Thing.TryGetComp<CompMaintainable>();
					compMaintainable.Maintained();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
