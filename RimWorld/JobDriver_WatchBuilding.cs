using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_WatchBuilding : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			yield return Toils_Reserve.Reserve(TargetIndex.A, base.CurJob.def.joyMaxParticipants);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			bool hasBed = base.TargetC.HasThing && base.TargetC.Thing is Building_Bed;
			Toil watch;
			if (hasBed)
			{
				this.KeepLyingDown(TargetIndex.C);
				yield return Toils_Reserve.Reserve(TargetIndex.C, ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount);
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.C);
				watch = Toils_LayDown.LayDown(TargetIndex.C, true, false, true, true);
				watch.AddFailCondition(() => !this.<watch>__1.actor.Awake());
			}
			else
			{
				if (base.TargetC.HasThing)
				{
					yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
				}
				yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
				watch = new Toil();
			}
			watch.AddPreTickAction(delegate
			{
				this.<>f__this.WatchTickAction();
			});
			watch.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.<>f__this.pawn);
			});
			watch.defaultCompleteMode = ToilCompleteMode.Delay;
			watch.defaultDuration = base.CurJob.def.joyDuration;
			yield return watch;
		}

		protected virtual void WatchTickAction()
		{
			this.pawn.Drawer.rotator.FaceCell(base.TargetA.Cell);
			this.pawn.GainComfortFromCellIfPossible();
			float statValue = base.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
			float extraJoyGainFactor = statValue;
			JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
		}
	}
}
