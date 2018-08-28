using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SmoothWall : JobDriver
	{
		private float workLeft = -1000f;

		protected int BaseWorkAmount
		{
			get
			{
				return 6500;
			}
		}

		protected DesignationDef DesDef
		{
			get
			{
				return DesignationDefOf.SmoothWall;
			}
		}

		protected StatDef SpeedStat
		{
			get
			{
				return StatDefOf.SmoothingSpeed;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.job.targetA;
			Job job = this.job;
			bool arg_62_0;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = this.job.targetA.Cell;
				job = this.job;
				arg_62_0 = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
			}
			else
			{
				arg_62_0 = false;
			}
			return arg_62_0;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.$this.job.ignoreDesignations && this.$this.Map.designationManager.DesignationAt(this.$this.TargetLocA, this.$this.DesDef) == null);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
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
					doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f, false);
				}
				if (this.$this.workLeft <= 0f)
				{
					this.$this.DoEffect();
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
			doWork.activeSkill = (() => SkillDefOf.Construction);
			yield return doWork;
		}

		protected void DoEffect()
		{
			SmoothableWallUtility.Notify_SmoothedByPawn(SmoothableWallUtility.SmoothWall(base.TargetA.Thing, this.pawn), this.pawn);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
		}
	}
}
