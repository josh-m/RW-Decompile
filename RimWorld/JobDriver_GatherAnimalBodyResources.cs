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

		private float gatherProgress;

		protected abstract float WorkTotal
		{
			get;
		}

		protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.gatherProgress, "gatherProgress", 0f, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil wait = new Toil();
			wait.initAction = delegate
			{
				Pawn actor = this.<wait>__0.actor;
				Pawn pawn = (Pawn)this.<wait>__0.actor.CurJob.GetTarget(TargetIndex.A).Thing;
				actor.pather.StopDead();
				PawnUtility.ForceWait(pawn, 15000, null, true);
			};
			wait.tickAction = delegate
			{
				Pawn actor = this.<wait>__0.actor;
				actor.skills.Learn(SkillDefOf.Animals, 0.142999992f, false);
				this.<>f__this.gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed, true);
				if (this.<>f__this.gatherProgress >= this.<>f__this.WorkTotal)
				{
					this.<>f__this.GetComp((Pawn)((Thing)this.<>f__this.CurJob.GetTarget(TargetIndex.A))).Gathered(this.<>f__this.pawn);
					actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
			};
			wait.AddFinishAction(delegate
			{
				Pawn pawn = (Pawn)this.<wait>__0.actor.CurJob.GetTarget(TargetIndex.A).Thing;
				if (pawn.jobs.curJob.def == JobDefOf.WaitMaintainPosture)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			});
			wait.FailOnDespawnedOrNull(TargetIndex.A);
			wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			wait.AddEndCondition(delegate
			{
				if (!this.<>f__this.GetComp((Pawn)((Thing)this.<>f__this.CurJob.GetTarget(TargetIndex.A))).ActiveAndFull)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			wait.WithProgressBar(TargetIndex.A, () => this.<>f__this.gatherProgress / this.<>f__this.WorkTotal, false, -0.5f);
			yield return wait;
		}
	}
}
