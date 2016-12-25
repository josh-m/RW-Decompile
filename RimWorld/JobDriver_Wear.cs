using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Wear : JobDriver
	{
		private const int DurationTicks = 60;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			Toil gotoApparel = new Toil();
			gotoApparel.initAction = delegate
			{
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetThingA, PathEndMode.ClosestTouch);
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
					Apparel apparel = (Apparel)this.<>f__this.CurJob.targetA.Thing;
					this.<>f__this.pawn.apparel.Wear(apparel, true);
					if (this.<>f__this.pawn.outfits != null && this.<>f__this.CurJob.playerForced)
					{
						this.<>f__this.pawn.outfits.forcedHandler.SetForced(apparel, true);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
