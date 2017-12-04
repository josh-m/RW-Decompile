using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FillFermentingBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex WortInd = TargetIndex.B;

		private const int Duration = 200;

		protected Building_FermentingBarrel Barrel
		{
			get
			{
				return (Building_FermentingBarrel)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Wort
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Barrel, this.job, 1, -1, null) && this.pawn.Reserve(this.Wort, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			base.AddEndCondition(() => (this.$this.Barrel.SpaceLeftForWort > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
			yield return Toils_General.DoAtomic(delegate
			{
				this.$this.job.count = this.$this.Barrel.SpaceLeftForWort;
			});
			Toil reserveWort = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return reserveWort;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveWort, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.Barrel.AddWort(this.$this.Wort);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
