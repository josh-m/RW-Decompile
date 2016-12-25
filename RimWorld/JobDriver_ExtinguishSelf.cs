using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExtinguishSelf : JobDriver
	{
		protected const int NumSpeechesToSay = 5;

		protected Fire TargetFire
		{
			get
			{
				return (Fire)base.CurJob.targetA.Thing;
			}
		}

		public override string GetReport()
		{
			if (this.TargetFire.parent != null)
			{
				return "ReportExtinguishingFireOn".Translate(new object[]
				{
					this.TargetFire.parent.LabelCap
				});
			}
			return "ReportExtinguishingFire".Translate();
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil extinguishDelay = new Toil();
			extinguishDelay.initAction = delegate
			{
			};
			extinguishDelay.defaultCompleteMode = ToilCompleteMode.Delay;
			extinguishDelay.defaultDuration = 150;
			yield return extinguishDelay;
			yield return new Toil
			{
				initAction = delegate
				{
					if (!this.<>f__this.TargetFire.Destroyed)
					{
						this.<>f__this.TargetFire.Destroy(DestroyMode.Vanish);
						this.<>f__this.pawn.records.Increment(RecordDefOf.FiresExtinguished);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
