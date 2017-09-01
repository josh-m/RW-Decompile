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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.<>f__this.CurJob.ignoreDesignations && this.<>f__this.Map.designationManager.DesignationAt(this.<>f__this.TargetLocA, this.<>f__this.DesDef) == null);
			ReservationLayerDef floor = ReservationLayerDefOf.Floor;
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, floor);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				this.<>f__this.workLeft = (float)this.<>f__this.BaseWorkAmount;
			};
			doWork.tickAction = delegate
			{
				float num = (this.<>f__this.SpeedStat == null) ? 1f : this.<doWork>__0.actor.GetStatValue(this.<>f__this.SpeedStat, true);
				this.<>f__this.workLeft -= num;
				if (this.<doWork>__0.actor.skills != null)
				{
					this.<doWork>__0.actor.skills.Learn(SkillDefOf.Construction, 0.22f, false);
				}
				if (this.<>f__this.clearSnow)
				{
					this.<>f__this.Map.snowGrid.SetDepth(this.<>f__this.TargetLocA, 0f);
				}
				if (this.<>f__this.workLeft <= 0f)
				{
					this.<>f__this.DoEffect(this.<>f__this.TargetLocA);
					Designation designation = this.<>f__this.Map.designationManager.DesignationAt(this.<>f__this.TargetLocA, this.<>f__this.DesDef);
					if (designation != null)
					{
						designation.Delete();
					}
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.WithProgressBar(TargetIndex.A, () => 1f - this.<>f__this.workLeft / (float)this.<>f__this.BaseWorkAmount, false, -0.5f);
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
