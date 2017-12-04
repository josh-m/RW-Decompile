using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RearmTrap : JobDriver
	{
		private const int RearmTicks = 1125;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.RearmTrap);
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.$this.pawn.pather.StartPath(this.$this.TargetThingA, PathEndMode.Touch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_General.Wait(1125).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.$this.job.targetA.Thing;
					Designation designation = this.$this.Map.designationManager.DesignationOn(thing, DesignationDefOf.RearmTrap);
					if (designation != null)
					{
						designation.Delete();
					}
					Building_TrapRearmable building_TrapRearmable = thing as Building_TrapRearmable;
					building_TrapRearmable.Rearm();
					this.$this.pawn.records.Increment(RecordDefOf.TrapsRearmed);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
