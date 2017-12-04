using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_WatchBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			if (!this.pawn.Reserve(this.job.targetA, this.job, this.job.def.joyMaxParticipants, 0, null))
			{
				return false;
			}
			if (!this.pawn.Reserve(this.job.targetB, this.job, 1, -1, null))
			{
				return false;
			}
			if (base.TargetC.HasThing)
			{
				if (base.TargetC.Thing is Building_Bed)
				{
					if (!this.pawn.Reserve(this.job.targetC, this.job, ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount, 0, null))
					{
						return false;
					}
				}
				else if (!this.pawn.Reserve(this.job.targetC, this.job, 1, -1, null))
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return base.TargetC.HasThing && base.TargetC.Thing is Building_Bed && JobInBedUtility.InBedOrRestSpotNow(this.pawn, base.TargetC);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);
			bool hasBed = base.TargetC.HasThing && base.TargetC.Thing is Building_Bed;
			Toil watch;
			if (hasBed)
			{
				this.KeepLyingDown(TargetIndex.C);
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.C);
				watch = Toils_LayDown.LayDown(TargetIndex.C, true, false, true, true);
				watch.AddFailCondition(() => !watch.actor.Awake());
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
				watch = new Toil();
			}
			watch.AddPreTickAction(delegate
			{
				this.$this.WatchTickAction();
			});
			watch.AddFinishAction(delegate
			{
				JoyUtility.TryGainRecRoomThought(this.$this.pawn);
			});
			watch.defaultCompleteMode = ToilCompleteMode.Delay;
			watch.defaultDuration = this.job.def.joyDuration;
			watch.handlingFacing = true;
			yield return watch;
		}

		protected virtual void WatchTickAction()
		{
			this.pawn.rotationTracker.FaceCell(base.TargetA.Cell);
			this.pawn.GainComfortFromCellIfPossible();
			float statValue = base.TargetThingA.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
			Pawn pawn = this.pawn;
			float extraJoyGainFactor = statValue;
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
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
