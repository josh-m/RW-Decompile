using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ClearSnow : JobDriver
	{
		private const float ClearWorkPerSnowDepth = 100f;

		private float workDone;

		private float TotalNeededWork
		{
			get
			{
				return 100f * base.Map.snowGrid.GetDepth(base.TargetLocA);
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil clearToil = new Toil();
			clearToil.tickAction = delegate
			{
				Pawn actor = this.<clearToil>__0.actor;
				float statValue = actor.GetStatValue(StatDefOf.WorkSpeedGlobal, true);
				float num = statValue;
				this.<>f__this.workDone += num;
				if (this.<>f__this.workDone >= this.<>f__this.TotalNeededWork)
				{
					this.<>f__this.Map.snowGrid.SetDepth(this.<>f__this.TargetLocA, 0f);
					this.<>f__this.ReadyForNextToil();
					return;
				}
			};
			clearToil.defaultCompleteMode = ToilCompleteMode.Never;
			clearToil.WithEffect(EffecterDefOf.ClearSnow, TargetIndex.A);
			clearToil.PlaySustainerOrSound(() => SoundDefOf.Interact_ClearSnow);
			clearToil.WithProgressBar(TargetIndex.A, () => this.<>f__this.workDone / this.<>f__this.TotalNeededWork, true, -0.5f);
			yield return clearToil;
		}
	}
}
