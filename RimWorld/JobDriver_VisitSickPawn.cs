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
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Thing Chair
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => !this.<>f__this.Patient.InBed() || !this.<>f__this.Patient.Awake());
			if (this.Chair != null)
			{
				this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			}
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			if (this.Chair != null)
			{
				yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
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
					this.<>f__this.Patient.needs.joy.GainJoy(this.<>f__this.CurJob.def.joyGainRate * 0.000144f, this.<>f__this.CurJob.def.joyKind);
					if (this.<>f__this.pawn.IsHashIntervalTick(320))
					{
						InteractionDef intDef = (Rand.Value >= 0.8f) ? InteractionDefOf.DeepTalk : InteractionDefOf.Chitchat;
						this.<>f__this.pawn.interactions.TryInteractWith(this.<>f__this.Patient, intDef);
					}
					this.<>f__this.pawn.Drawer.rotator.FaceCell(this.<>f__this.Patient.Position);
					this.<>f__this.pawn.GainComfortFromCellIfPossible();
					JoyUtility.JoyTickCheckEnd(this.<>f__this.pawn, JoyTickFullJoyAction.None, 1f);
					if (this.<>f__this.pawn.needs.joy.CurLevelPercentage > 0.9999f && this.<>f__this.Patient.needs.joy.CurLevelPercentage > 0.9999f)
					{
						this.<>f__this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				},
				socialMode = RandomSocialMode.Off,
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = base.CurJob.def.joyDuration
			};
		}
	}
}
