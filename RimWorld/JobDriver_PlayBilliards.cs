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

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo targetA = this.job.targetA;
			Job job = this.job;
			int joyMaxParticipants = this.job.def.joyMaxParticipants;
			int stackCount = 0;
			return pawn.Reserve(targetA, job, joyMaxParticipants, stackCount, null, errorOnFailed);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			Toil chooseCell = Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.B);
			yield return chooseCell;
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			Toil play = new Toil();
			play.initAction = delegate
			{
				this.$this.job.locomotionUrgency = LocomotionUrgency.Walk;
			};
			play.tickAction = delegate
			{
				this.$this.pawn.rotationTracker.FaceCell(this.$this.TargetA.Thing.OccupiedRect().ClosestCellTo(this.$this.pawn.Position));
				if (this.$this.ticksLeftThisToil == 300)
				{
					SoundDefOf.PlayBilliards.PlayOneShot(new TargetInfo(this.$this.pawn.Position, this.$this.pawn.Map, false));
				}
				if (Find.TickManager.TicksGame > this.$this.startTick + this.$this.job.def.joyDuration)
				{
					this.$this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				Pawn pawn = this.$this.pawn;
				Building joySource = (Building)this.$this.TargetThingA;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, joySource);
			};
			play.handlingFacing = true;
			play.socialMode = RandomSocialMode.SuperActive;
			play.defaultCompleteMode = ToilCompleteMode.Delay;
			play.defaultDuration = 600;
			play.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.$this.pawn);
			});
			yield return play;
			yield return Toils_Reserve.Release(TargetIndex.B);
			yield return Toils_Jump.Jump(chooseCell);
		}

		public override object[] TaleParameters()
		{
			return new object[]
			{
				this.pawn,
				base.TargetA.Thing.def
			};
		}
	}
}
