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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.RearmTrap);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetThingA, PathEndMode.Touch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_General.Wait(1125).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.<>f__this.CurJob.targetA.Thing;
					Designation designation = Find.DesignationManager.DesignationOn(thing, DesignationDefOf.RearmTrap);
					if (designation != null)
					{
						designation.Delete();
					}
					Building_TrapRearmable building_TrapRearmable = thing as Building_TrapRearmable;
					building_TrapRearmable.Rearm();
					this.<>f__this.pawn.records.Increment(RecordDefOf.TrapsRearmed);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
