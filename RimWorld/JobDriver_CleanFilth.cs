using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CleanFilth : JobDriver
	{
		private const TargetIndex FilthInd = TargetIndex.A;

		private float cleaningWorkDone;

		private float totalCleaningWorkDone;

		private float totalCleaningWorkRequired;

		private Filth Filth
		{
			get
			{
				return (Filth)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.ReserveQueue(TargetIndex.A, 1);
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
			Toil checkNextQueuedTarget = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, checkNextQueuedTarget).JumpIfOutsideHomeArea(TargetIndex.A, checkNextQueuedTarget);
			Toil clean = new Toil();
			clean.initAction = delegate
			{
				this.<>f__this.cleaningWorkDone = 0f;
				this.<>f__this.totalCleaningWorkDone = 0f;
				this.<>f__this.totalCleaningWorkRequired = this.<>f__this.Filth.def.filth.cleaningWorkToReduceThickness * (float)this.<>f__this.Filth.thickness;
			};
			clean.tickAction = delegate
			{
				Filth filth = this.<>f__this.Filth;
				this.<>f__this.cleaningWorkDone += 1f;
				this.<>f__this.totalCleaningWorkDone += 1f;
				if (this.<>f__this.cleaningWorkDone > filth.def.filth.cleaningWorkToReduceThickness)
				{
					filth.ThinFilth();
					this.<>f__this.cleaningWorkDone = 0f;
					if (filth.Destroyed)
					{
						this.<clean>__2.actor.records.Increment(RecordDefOf.MessesCleaned);
						this.<>f__this.ReadyForNextToil();
						return;
					}
				}
			};
			clean.defaultCompleteMode = ToilCompleteMode.Never;
			clean.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
			clean.WithProgressBar(TargetIndex.A, () => this.<>f__this.totalCleaningWorkDone / this.<>f__this.totalCleaningWorkRequired, true, -0.5f);
			clean.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
			clean.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, checkNextQueuedTarget);
			clean.JumpIfOutsideHomeArea(TargetIndex.A, checkNextQueuedTarget);
			yield return clean;
			yield return checkNextQueuedTarget;
			yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.A, initExtractTargetFromQueue);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.cleaningWorkDone, "cleaningWorkDone", 0f, false);
			Scribe_Values.LookValue<float>(ref this.totalCleaningWorkDone, "totalCleaningWorkDone", 0f, false);
			Scribe_Values.LookValue<float>(ref this.totalCleaningWorkRequired, "totalCleaningWorkRequired", 0f, false);
		}
	}
}
