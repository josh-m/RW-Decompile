using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ClearSnow : JobDriver
	{
		private float workDone;

		private const float ClearWorkPerSnowDepth = 100f;

		private float TotalNeededWork
		{
			get
			{
				return 100f * base.Map.snowGrid.GetDepth(base.TargetLocA);
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil clearToil = new Toil();
			clearToil.tickAction = delegate
			{
				Pawn actor = clearToil.actor;
				float statValue = actor.GetStatValue(StatDefOf.WorkSpeedGlobal, true);
				float num = statValue;
				this.$this.workDone += num;
				if (this.$this.workDone >= this.$this.TotalNeededWork)
				{
					this.$this.Map.snowGrid.SetDepth(this.$this.TargetLocA, 0f);
					this.$this.ReadyForNextToil();
					return;
				}
			};
			clearToil.defaultCompleteMode = ToilCompleteMode.Never;
			clearToil.WithEffect(EffecterDefOf.ClearSnow, TargetIndex.A);
			clearToil.PlaySustainerOrSound(() => SoundDefOf.Interact_ClearSnow);
			clearToil.WithProgressBar(TargetIndex.A, () => this.$this.workDone / this.$this.TotalNeededWork, true, -0.5f);
			clearToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return clearToil;
		}
	}
}
