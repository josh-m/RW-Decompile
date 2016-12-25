using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_RemoveBuilding : JobDriver
	{
		private float workLeft;

		private float totalNeededWork;

		protected Thing Target
		{
			get
			{
				return base.CurJob.targetA.Thing;
			}
		}

		protected Building Building
		{
			get
			{
				return (Building)this.Target.GetInnerIfMinified();
			}
		}

		protected abstract DesignationDef Designation
		{
			get;
		}

		protected abstract int TotalNeededWork
		{
			get;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.workLeft, "workLeft", 0f, false);
			Scribe_Values.LookValue<float>(ref this.totalNeededWork, "totalNeededWork", 0f, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnThingMissingDesignation(TargetIndex.A, this.Designation);
			this.FailOnForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil().FailOnDestroyedNullOrForbidden(TargetIndex.A);
			doWork.initAction = delegate
			{
				this.<>f__this.totalNeededWork = (float)this.<>f__this.TotalNeededWork;
				this.<>f__this.workLeft = this.<>f__this.totalNeededWork;
			};
			doWork.tickAction = delegate
			{
				this.<>f__this.workLeft -= this.<>f__this.pawn.GetStatValue(StatDefOf.ConstructionSpeed, true);
				this.<>f__this.TickAction();
				if (this.<>f__this.workLeft <= 0f)
				{
					this.<doWork>__0.actor.jobs.curDriver.ReadyForNextToil();
				}
			};
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.WithProgressBar(TargetIndex.A, () => 1f - this.<>f__this.workLeft / this.<>f__this.totalNeededWork, false, -0.5f);
			yield return doWork;
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.FinishedRemoving();
					Find.DesignationManager.RemoveAllDesignationsOn(this.<>f__this.Target, false);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		protected virtual void FinishedRemoving()
		{
		}

		protected virtual void TickAction()
		{
		}
	}
}
