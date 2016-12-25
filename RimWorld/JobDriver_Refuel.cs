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

		private const int RefuelingDuration = 300;

		protected Thing Refuelable
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Fuel
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
			this.FailOn(delegate
			{
				TargetInfo target = this.<>f__this.pawn.CurJob.GetTarget(TargetIndex.A);
				ThingWithComps thingWithComps = target.Thing as ThingWithComps;
				if (thingWithComps != null)
				{
					CompFlickable comp = thingWithComps.GetComp<CompFlickable>();
					if (comp != null && (!comp.SwitchIsOn || Find.DesignationManager.DesignationOn(target.Thing, DesignationDefOf.Flick) != null))
					{
						return true;
					}
				}
				return false;
			});
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return reserveFuel;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(300).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
		}
	}
}
