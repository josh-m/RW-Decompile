using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseItem : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = Toils_General.Wait(100);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<use>__1.actor;
					CompUsable compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable>();
					compUsable.UsedBy(actor);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
