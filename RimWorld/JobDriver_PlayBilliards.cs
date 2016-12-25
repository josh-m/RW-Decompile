using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_PlayBilliards : JobDriver
	{
		private const int ShotDuration = 600;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			yield return Toils_Reserve.Reserve(TargetIndex.A, base.CurJob.def.joyMaxParticipants);
			Toil chooseCell = new Toil();
			chooseCell.initAction = delegate
			{
				int num = 0;
				while (true)
				{
					this.<>f__this.CurJob.targetB = this.<>f__this.CurJob.targetA.Thing.RandomAdjacentCell8Way();
					num++;
					if (num > 100)
					{
						break;
					}
					if (this.<>f__this.pawn.CanReserve((IntVec3)this.<>f__this.CurJob.targetB, 1))
					{
						return;
					}
				}
				Log.Error(this.<>f__this.pawn + " could not find cell adjacent to billiards table " + this.<>f__this.TargetThingA);
				this.<>f__this.EndJobWith(JobCondition.Errored);
			};
			yield return chooseCell;
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil play = new Toil();
			play.initAction = delegate
			{
				this.<>f__this.CurJob.locomotionUrgency = LocomotionUrgency.Walk;
			};
			play.tickAction = delegate
			{
				this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.TargetA.Thing.OccupiedRect().ClosestCellTo(this.<>f__this.pawn.Position));
				if (this.<>f__this.pawn.jobs.curDriver.ticksLeftThisToil == 300)
				{
					SoundDefOf.PlayBilliards.PlayOneShot(this.<>f__this.pawn.Position);
				}
				if (Find.TickManager.TicksGame > this.<>f__this.startTick + this.<>f__this.CurJob.def.joyDuration)
				{
					this.<>f__this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				float statValue = this.<>f__this.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				float extraJoyGainFactor = statValue;
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
			play.socialMode = RandomSocialMode.SuperActive;
			play.defaultCompleteMode = ToilCompleteMode.Delay;
			play.defaultDuration = 600;
			play.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.<>f__this.pawn);
			});
			yield return play;
			yield return Toils_Reserve.Release(TargetIndex.B);
			yield return Toils_Jump.Jump(chooseCell);
		}
	}
}
