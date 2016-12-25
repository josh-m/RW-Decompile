using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_AffectRoof : JobDriver
	{
		private const TargetIndex CellInd = TargetIndex.A;

		private const TargetIndex GotoTargetInd = TargetIndex.B;

		private const float BaseWorkAmount = 65f;

		private float workLeft;

		protected IntVec3 Cell
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Cell;
			}
		}

		protected abstract PathEndMode PathEndMode
		{
			get;
		}

		protected abstract void DoEffect();

		protected abstract bool DoWorkFailOn();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.workLeft, "workLeft", 0f, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.Goto(TargetIndex.B, this.PathEndMode);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				this.<>f__this.workLeft = 65f;
			};
			doWork.tickAction = delegate
			{
				float statValue = this.<doWork>__0.actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				this.<>f__this.workLeft -= statValue;
				if (this.<doWork>__0.actor.skills != null)
				{
					this.<doWork>__0.actor.skills.Learn(SkillDefOf.Construction, 0.11f, false);
				}
				if (this.<>f__this.workLeft <= 0f)
				{
					this.<>f__this.DoEffect();
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			doWork.PlaySoundAtStart(SoundDefOf.RoofStart);
			doWork.PlaySoundAtEnd(SoundDefOf.RoofFinish);
			doWork.WithEffect(EffecterDefOf.RoofWork, TargetIndex.A);
			doWork.FailOn(new Func<bool>(this.DoWorkFailOn));
			doWork.WithProgressBar(TargetIndex.A, () => 1f - this.<>f__this.workLeft / 65f, false, -0.5f);
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			yield return doWork;
		}
	}
}
