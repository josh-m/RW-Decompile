using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Refuel : JobDriver
	{
		private const TargetIndex RefuelableInd = TargetIndex.A;

		private const TargetIndex FuelInd = TargetIndex.B;

		private const int RefuelingDuration = 240;

		protected Thing Refuelable
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected CompRefuelable RefuelableComp
		{
			get
			{
				return this.Refuelable.TryGetComp<CompRefuelable>();
			}
		}

		protected Thing Fuel
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Refuelable, this.job, 1, -1, null) && this.pawn.Reserve(this.Fuel, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				ThingWithComps thingWithComps = this.$this.job.GetTarget(TargetIndex.A).Thing as ThingWithComps;
				if (thingWithComps != null)
				{
					CompFlickable comp = thingWithComps.GetComp<CompFlickable>();
					if (comp != null && !comp.SwitchIsOn)
					{
						return true;
					}
				}
				return false;
			});
			base.AddEndCondition(() => (!this.$this.RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
			yield return Toils_General.DoAtomic(delegate
			{
				this.$this.job.count = this.$this.RefuelableComp.GetFuelCountToFullyRefuel();
			});
			Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return reserveFuel;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
		}
	}
}
