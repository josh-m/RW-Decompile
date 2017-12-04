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

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, this.job.def.joyMaxParticipants, 0, null);
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
				float statValue = this.$this.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				Pawn pawn = this.$this.pawn;
				float extraJoyGainFactor = statValue;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
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
