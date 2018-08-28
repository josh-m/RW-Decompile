using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SitFacingBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.job.targetA;
			Job job = this.job;
			int joyMaxParticipants = this.job.def.joyMaxParticipants;
			int stackCount = 0;
			bool arg_71_0;
			if (pawn.Reserve(target, job, joyMaxParticipants, stackCount, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = this.job.targetB;
				job = this.job;
				arg_71_0 = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
			}
			else
			{
				arg_71_0 = false;
			}
			return arg_71_0;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
			Toil play = new Toil();
			play.tickAction = delegate
			{
				this.$this.pawn.rotationTracker.FaceTarget(this.$this.TargetA);
				this.$this.pawn.GainComfortFromCellIfPossible();
				Pawn pawn = this.$this.pawn;
				Building joySource = (Building)this.$this.TargetThingA;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, joySource);
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
