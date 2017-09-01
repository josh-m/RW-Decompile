using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mate : JobDriver
	{
		private const int MateDuration = 500;

		private const TargetIndex FemInd = TargetIndex.A;

		private const int TicksBetweenHeartMotes = 100;

		private Pawn Female
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = Toils_General.WaitWith(TargetIndex.A, 500, false, false);
			prepare.tickAction = delegate
			{
				if (this.<>f__this.pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(this.<>f__this.pawn.Position, this.<>f__this.pawn.Map, ThingDefOf.Mote_Heart);
				}
				if (this.<>f__this.Female.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(this.<>f__this.Female.Position, this.<>f__this.pawn.Map, ThingDefOf.Mote_Heart);
				}
			};
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<finalize>__1.actor;
					Pawn female = this.<>f__this.Female;
					PawnUtility.Mated(actor, female);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
