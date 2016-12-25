using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Nuzzle : JobDriver
	{
		private const int NuzzleDuration = 100;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					PawnUtility.ForceWait((Pawn)this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing, 100, null);
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 100
			};
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<finalize>__1.actor;
					Pawn recipient = (Pawn)actor.CurJob.targetA.Thing;
					actor.interactions.TryInteractWith(recipient, InteractionDefOf.Nuzzle);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
