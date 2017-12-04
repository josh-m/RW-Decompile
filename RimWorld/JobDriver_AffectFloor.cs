using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_AffectFloor : JobDriver
	{
		private float workLeft = -1000f;

		protected bool clearSnow;

		protected abstract int BaseWorkAmount
		{
			get;
		}

		protected abstract DesignationDef DesDef
		{
			get;
		}

		protected virtual StatDef SpeedStat
		{
			get
			{
				return null;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo targetA = this.job.targetA;
			Job job = this.job;
			ReservationLayerDef floor = ReservationLayerDefOf.Floor;
			return pawn.Reserve(targetA, job, 1, -1, floor);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.$this.job.ignoreDesignations && this.$this.Map.designationManager.DesignationAt(this.$this.TargetLocA, this.$this.DesDef) == null);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				this.$this.workLeft = (float)this.$this.BaseWorkAmount;
			};
			doWork.tickAction = delegate
			{
				float num = (this.$this.SpeedStat == null) ? 1f : doWork.actor.GetStatValue(this.$this.SpeedStat, true);
				this.$this.workLeft -= num;
				if (doWork.actor.skills != null)
				{
					doWork.actor.skills.Learn(SkillDefOf.Construction, 0.11f, false);
				}
				if (this.$this.clearSnow)
				{
					this.$this.Map.snowGrid.SetDepth(this.$this.TargetLocA, 0f);
				}
				if (this.$this.workLeft <= 0f)
				{
					this.$this.DoEffect(this.$this.TargetLocA);
					Designation designation = this.$this.Map.designationManager.DesignationAt(this.$this.TargetLocA, this.$this.DesDef);
					if (designation != null)
					{
						designation.Delete();
					}
					this.$this.ReadyForNextToil();
					return;
				}
			};
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.WithProgressBar(TargetIndex.A, () => 1f - this.$this.workLeft / (float)this.$this.BaseWorkAmount, false, -0.5f);
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			yield return doWork;
		}

		protected abstract void DoEffect(IntVec3 c);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
		}
	}
}
