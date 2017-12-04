using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CarryToCryptosleepCasket : JobDriver
	{
		private const TargetIndex TakeeInd = TargetIndex.A;

		private const TargetIndex DropPodInd = TargetIndex.B;

		protected Pawn Takee
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Building_CryptosleepCasket DropPod
		{
			get
			{
				return (Building_CryptosleepCasket)this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Takee, this.job, 1, -1, null) && this.pawn.Reserve(this.DropPod, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !this.$this.DropPod.Accepts(this.$this.Takee));
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.$this.DropPod.GetDirectlyHeldThings().Count > 0).FailOn(() => !this.$this.Takee.Downed).FailOn(() => !this.$this.pawn.CanReach(this.$this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
			Toil prepare = Toils_General.Wait(500);
			prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
			prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.DropPod.TryAcceptThing(this.$this.Takee, true);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		public override object[] TaleParameters()
		{
			return new object[]
			{
				this.pawn,
				this.Takee
			};
		}
	}
}
