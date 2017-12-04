using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SitFacingBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, this.job.def.joyMaxParticipants, 0, null) && this.pawn.Reserve(this.job.targetB, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			this.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			Toil play = new Toil();
			play.tickAction = delegate
			{
				this.$this.pawn.rotationTracker.FaceCell(this.$this.TargetA.Cell);
				this.$this.pawn.GainComfortFromCellIfPossible();
				float statValue = this.$this.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				Pawn pawn = this.$this.pawn;
				float extraJoyGainFactor = statValue;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
			play.handlingFacing = true;
			play.defaultCompleteMode = ToilCompleteMode.Delay;
			play.defaultDuration = this.job.def.joyDuration;
			play.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.$this.pawn);
			});
			this.ModifyPlayToil(play);
			yield return play;
		}

		protected virtual void ModifyPlayToil(Toil toil)
		{
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
