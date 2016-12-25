using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_ReleasePrisoner : JobDriver
	{
		private const TargetIndex PrisonerInd = TargetIndex.A;

		private const TargetIndex ReleaseCellInd = TargetIndex.B;

		private Pawn Prisoner
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			this.FailOn(() => ((Pawn)((Thing)this.<>f__this.GetActor().CurJob.GetTarget(TargetIndex.A))).guest.interactionMode != PrisonerInteractionMode.Release);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return reserveTargetA;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => !this.<>f__this.Prisoner.IsPrisonerOfColony || !this.<>f__this.Prisoner.guest.PrisonerIsSecure).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, false);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<setReleased>__2.actor;
					Job curJob = actor.jobs.curJob;
					Pawn p = curJob.targetA.Thing as Pawn;
					GenGuest.PrisonerRelease(p);
				}
			};
		}
	}
}
