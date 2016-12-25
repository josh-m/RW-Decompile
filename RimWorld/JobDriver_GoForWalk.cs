using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GoForWalk : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.<>f__this.pawn, null));
			Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			goToil.tickAction = delegate
			{
				if (Find.TickManager.TicksGame > this.<>f__this.startTick + this.<>f__this.CurJob.def.joyDuration)
				{
					this.<>f__this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, 1f);
			};
			yield return goToil;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.CurJob.targetQueueA.Count > 0)
					{
						LocalTargetInfo targetA = this.<>f__this.CurJob.targetQueueA[0];
						this.<>f__this.CurJob.targetQueueA.RemoveAt(0);
						this.<>f__this.CurJob.targetA = targetA;
						this.<>f__this.JumpToToil(this.<goToil>__0);
						return;
					}
				}
			};
		}
	}
}
