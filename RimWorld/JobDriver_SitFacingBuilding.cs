using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SitFacingBuilding : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			this.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
			yield return Toils_Reserve.Reserve(TargetIndex.A, base.CurJob.def.joyMaxParticipants);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			Toil play = new Toil();
			play.tickAction = delegate
			{
				this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.TargetA.Cell);
				this.<>f__this.pawn.GainComfortFromCellIfPossible();
				float statValue = this.<>f__this.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				float extraJoyGainFactor = statValue;
				JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
			play.defaultCompleteMode = ToilCompleteMode.Delay;
			play.defaultDuration = base.CurJob.def.joyDuration;
			play.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.<>f__this.pawn);
			});
			yield return play;
		}
	}
}
