using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_VisitJoyThing : JobDriver
	{
		protected const TargetIndex TargetThingIndex = TargetIndex.A;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil wait = Toils_General.Wait(base.CurJob.def.joyDuration);
			wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			wait.tickAction = this.GetWaitTickAction();
			yield return wait;
		}

		protected abstract Action GetWaitTickAction();
	}
}
