using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitSickPawn : JobDriver
	{
		private const TargetIndex PatientInd = TargetIndex.A;

		private const TargetIndex ChairInd = TargetIndex.B;

		private Pawn Patient
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Thing Chair
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.Patient;
			Job job = this.job;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (this.Chair != null)
			{
				pawn = this.pawn;
				target = this.Chair;
				job = this.job;
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
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => !this.$this.Patient.InBed() || !this.$this.Patient.Awake());
			if (this.Chair != null)
			{
				this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			}
			if (this.Chair != null)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			}
			else
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			}
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
			yield return new Toil
			{
				tickAction = delegate
				{
					this.$this.Patient.needs.joy.GainJoy(this.$this.job.def.joyGainRate * 0.000144f, this.$this.job.def.joyKind);
					if (this.$this.pawn.IsHashIntervalTick(320))
					{
						InteractionDef intDef = (Rand.Value >= 0.8f) ? InteractionDefOf.DeepTalk : InteractionDefOf.Chitchat;
						this.$this.pawn.interactions.TryInteractWith(this.$this.Patient, intDef);
					}
					this.$this.pawn.rotationTracker.FaceCell(this.$this.Patient.Position);
					this.$this.pawn.GainComfortFromCellIfPossible();
					JoyUtility.JoyTickCheckEnd(this.$this.pawn, JoyTickFullJoyAction.None, 1f, null);
					if (this.$this.pawn.needs.joy.CurLevelPercentage > 0.9999f && this.$this.Patient.needs.joy.CurLevelPercentage > 0.9999f)
					{
						this.$this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				},
				handlingFacing = true,
				socialMode = RandomSocialMode.Off,
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = this.job.def.joyDuration
			};
		}
	}
}
