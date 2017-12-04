using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RelaxAlone : JobDriver
	{
		private Rot4 faceDir;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.faceDir = ((!this.$this.job.def.faceDir.IsValid) ? Rot4.Random : this.$this.job.def.faceDir);
				},
				tickAction = delegate
				{
					this.$this.pawn.rotationTracker.FaceCell(this.$this.pawn.Position + this.$this.faceDir.FacingCell);
					this.$this.pawn.GainComfortFromCellIfPossible();
					JoyUtility.JoyTickCheckEnd(this.$this.pawn, JoyTickFullJoyAction.EndJob, 1f);
				},
				handlingFacing = true,
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = this.job.def.joyDuration
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Rot4>(ref this.faceDir, "faceDir", default(Rot4), false);
		}
	}
}
