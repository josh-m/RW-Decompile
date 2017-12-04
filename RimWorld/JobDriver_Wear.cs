using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Wear : JobDriver
	{
		private const int DurationTicks = 60;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil gotoApparel = new Toil();
			gotoApparel.initAction = delegate
			{
				this.$this.pawn.pather.StartPath(this.$this.TargetThingA, PathEndMode.ClosestTouch);
			};
			gotoApparel.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoApparel.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoApparel;
			Toil prepare = new Toil();
			prepare.defaultCompleteMode = ToilCompleteMode.Delay;
			prepare.defaultDuration = 60;
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					Apparel apparel = (Apparel)this.$this.job.targetA.Thing;
					this.$this.pawn.apparel.Wear(apparel, true);
					if (this.$this.pawn.outfits != null && this.$this.job.playerForced)
					{
						this.$this.pawn.outfits.forcedHandler.SetForced(apparel, true);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
