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
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
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
				if (this.$this.pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(this.$this.pawn.Position, this.$this.pawn.Map, ThingDefOf.Mote_Heart);
				}
				if (this.$this.Female.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(this.$this.Female.Position, this.$this.pawn.Map, ThingDefOf.Mote_Heart);
				}
			};
			yield return prepare;
			yield return Toils_General.Do(delegate
			{
				PawnUtility.Mated(this.$this.pawn, this.$this.Female);
			});
		}
	}
}
