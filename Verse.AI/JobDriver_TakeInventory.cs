using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_TakeInventory : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.$this.pawn.pather.StartPath(this.$this.TargetThingA, PathEndMode.ClosestTouch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_Haul.TakeToInventory(TargetIndex.A, this.job.count);
		}
	}
}
