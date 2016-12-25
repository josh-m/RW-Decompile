using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_TakeInventory : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetThingA, PathEndMode.ClosestTouch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_Haul.TakeToInventory(TargetIndex.A, base.CurJob.count);
		}
	}
}
