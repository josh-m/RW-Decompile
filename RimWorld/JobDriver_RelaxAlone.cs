using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RelaxAlone : JobDriver
	{
		private Rot4 faceDir = Rot4.Invalid;

		private const TargetIndex SpotOrBedInd = TargetIndex.A;

		private bool FromBed
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).HasThing;
			}
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return this.FromBed && JobInBedUtility.InBedOrRestSpotNow(this.pawn, this.job.GetTarget(TargetIndex.A));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (this.FromBed)
			{
				Pawn pawn = this.pawn;
				LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
				Job job = this.job;
				int sleepingSlotsCount = ((Building_Bed)this.job.GetTarget(TargetIndex.A).Thing).SleepingSlotsCount;
				int stackCount = 0;
				if (!pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed))
				{
					return false;
				}
			}
			else
			{
				Pawn pawn = this.pawn;
				LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
				Job job = this.job;
				if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil relax;
			if (this.FromBed)
			{
				this.KeepLyingDown(TargetIndex.A);
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A, TargetIndex.None);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
				relax = Toils_LayDown.LayDown(TargetIndex.A, true, false, true, true);
				relax.AddFailCondition(() => !this.$this.pawn.Awake());
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
				relax = new Toil();
				relax.initAction = delegate
				{
					this.$this.faceDir = ((!this.$this.job.def.faceDir.IsValid) ? Rot4.Random : this.$this.job.def.faceDir);
				};
				relax.handlingFacing = true;
			}
			relax.defaultCompleteMode = ToilCompleteMode.Delay;
			relax.defaultDuration = this.job.def.joyDuration;
			relax.AddPreTickAction(delegate
			{
				if (this.$this.faceDir.IsValid)
				{
					this.$this.pawn.rotationTracker.FaceCell(this.$this.pawn.Position + this.$this.faceDir.FacingCell);
				}
				this.$this.pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(this.$this.pawn, JoyTickFullJoyAction.EndJob, 1f, null);
			});
			yield return relax;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Rot4>(ref this.faceDir, "faceDir", default(Rot4), false);
		}
	}
}
