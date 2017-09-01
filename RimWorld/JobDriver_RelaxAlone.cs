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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.faceDir = ((!this.<>f__this.CurJob.def.faceDir.IsValid) ? Rot4.Random : this.<>f__this.CurJob.def.faceDir);
				},
				tickAction = delegate
				{
					this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.pawn.Position + this.<>f__this.faceDir.FacingCell);
					this.<>f__this.pawn.GainComfortFromCellIfPossible();
					JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, 1f);
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = base.CurJob.def.joyDuration
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Rot4>(ref this.faceDir, "faceDir", default(Rot4), false);
		}
	}
}
