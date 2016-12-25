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
			yield return new Toil
			{
				initAction = delegate
				{
					PawnUtility.ForceWait(this.<>f__this.Female, 500, null);
				},
				tickAction = delegate
				{
					if (this.<>f__this.pawn.IsHashIntervalTick(100))
					{
						MoteMaker.ThrowMetaIcon(this.<>f__this.pawn.Position, ThingDefOf.Mote_Heart);
					}
					if (this.<>f__this.Female.IsHashIntervalTick(100))
					{
						MoteMaker.ThrowMetaIcon(this.<>f__this.Female.Position, ThingDefOf.Mote_Heart);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 500
			};
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
