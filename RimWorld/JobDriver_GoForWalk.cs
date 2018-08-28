using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GoForWalk : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.$this.pawn, null));
			Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			goToil.tickAction = delegate
			{
				if (Find.TickManager.TicksGame > this.$this.startTick + this.$this.job.def.joyDuration)
				{
					this.$this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				JoyUtility.JoyTickCheckEnd(this.$this.pawn, JoyTickFullJoyAction.EndJob, 1f, null);
			};
			yield return goToil;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.$this.job.targetQueueA.Count > 0)
					{
						LocalTargetInfo targetA = this.$this.job.targetQueueA[0];
						this.$this.job.targetQueueA.RemoveAt(0);
						this.$this.job.targetA = targetA;
						this.$this.JumpToToil(goToil);
						return;
					}
				}
			};
		}
	}
}
