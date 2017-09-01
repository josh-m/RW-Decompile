using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Strip : JobDriver
	{
		private const int StripTicks = 60;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !StrippableUtility.CanBeStrippedByColony(this.<>f__this.TargetThingA));
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			Toil gotoThing = new Toil();
			gotoThing.initAction = delegate
			{
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetThingA, PathEndMode.ClosestTouch);
			};
			gotoThing.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate
				{
					Thing thing = this.<>f__this.CurJob.targetA.Thing;
					Designation designation = this.<>f__this.Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip);
					if (designation != null)
					{
						designation.Delete();
					}
					IStrippable strippable = thing as IStrippable;
					if (strippable != null)
					{
						strippable.Strip();
					}
					this.<>f__this.pawn.records.Increment(RecordDefOf.BodiesStripped);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
