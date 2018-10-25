using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExtinguishSelf : JobDriver
	{
		protected Fire TargetFire
		{
			get
			{
				return (Fire)this.job.targetA.Thing;
			}
		}

		public override string GetReport()
		{
			if (this.TargetFire != null && this.TargetFire.parent != null)
			{
				return "ReportExtinguishingFireOn".Translate(this.TargetFire.parent.LabelCap, this.TargetFire.parent.Named("TARGET"));
			}
			return "ReportExtinguishingFire".Translate();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 150
			};
			Toil killFire = new Toil();
			killFire.initAction = delegate
			{
				this.$this.TargetFire.Destroy(DestroyMode.Vanish);
				this.$this.pawn.records.Increment(RecordDefOf.FiresExtinguished);
			};
			killFire.FailOnDestroyedOrNull(TargetIndex.A);
			killFire.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return killFire;
		}
	}
}
